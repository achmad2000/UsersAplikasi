using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Pengguna.Models;
using Pengguna.Data;
//using Pengguna.Hubs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace Pengguna.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<JobOrderHub> _hubContext;
        private readonly IWebHostEnvironment _environment;

        public CustomerController(ApplicationDbContext context, IHubContext<JobOrderHub> hubContext, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            _hubContext = hubContext;
        }
               public IActionResult OrderCustomer()
        {
            ViewData["ActivePage"] = "OrderCustomer";
            return View();
        }
            //profile
            [HttpGet]
        public IActionResult Profile()
        {
            // Pastikan user sudah login
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Ambil data user dari database
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["ActivePage"] = "Profile";
            return View(user);
        }
        [HttpPost]
        public IActionResult Profile(UserModel updatedUser, IFormFile? ProfileImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Update field basic
            user.Username = updatedUser.Username;
            user.PhoneNumber = updatedUser.PhoneNumber;

            // Upload foto profil
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = $"{Guid.NewGuid()}_{ProfileImage.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(fileStream);
                }

                user.ProfileImagePath = "/uploads/" + uniqueFileName;
            }
            _context.Users.Update(user);
            _context.SaveChanges();
            HttpContext.Session.SetString("Username", user.Username);

            TempData["Success"] = "Profil berhasil diperbarui!";
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath ?? "~/images/profile/default-profile.png");
            return RedirectToAction("Profile");

        }
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Index";
            return View();
        }
        public IActionResult Order()
        {
            ViewData["ActivePage"] = "Order";
            return View();
        }
       
        // Order Customer

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(WaitingResponOrder model) 
        {
            if (ModelState.IsValid)
            {
                var customerName = HttpContext.Session.GetString("Username");
                model.NamaCustomer = customerName;

                model.Status = "Menunggu Teknisi";
                model.IsTaken = false;
                model.NamaTeknisi = null;

                _context.WaitingResponOrders.Add(model);
                _context.SaveChanges();
                model.NomorService = $"HS{2000 + model.Id}";
                _context.Update(model);
                _context.SaveChanges();
                await _hubContext.Clients.Group("Technicians").SendAsync("UpdateReceived");

                return RedirectToAction("WaitingOrder");
            }
            return View("Order", model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id) 
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            bool isRequestingCancel = false; 

            if (order.Status?.Trim() == "Menunggu Teknisi")
            {
                order.Status = "Dibatalkan";
                TempData["Message"] = "Pesanan berhasil dibatalkan.";
            }
            else if (order.Status?.Trim() == "Diterima Teknisi" || order.Status?.Trim() == "Aktif (Lanjut Service)")
            {
                order.Status = "Menunggu Persetujuan Cancel";
                TempData["Message"] = "Permintaan pembatalan telah dikirim ke teknisi.";
                isRequestingCancel = true; 
            }
            _context.SaveChanges();
            if (isRequestingCancel)
            {
                await _hubContext.Clients.Group("Technicians").SendAsync("UpdateReceived");
            }

            return RedirectToAction("WaitingOrder");
        }

        public IActionResult WaitingOrder()
        {
            var customerName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(customerName))
            {
                return RedirectToAction("Login", "Account");
            }
            var myOrders = _context.WaitingResponOrders
                .Where(o => o.NamaCustomer != null &&
                            o.NamaCustomer.Trim().ToLower() == customerName.Trim().ToLower())
                .OrderByDescending(o => o.Id)
                .ToList();
            return View(myOrders);
        }
        [HttpPost]
        public IActionResult DeleteOrderLog(int id)
        {
            var customerName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(customerName))
            {
                return RedirectToAction("Login", "Account"); 
            }
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);

            if (order != null && order.NamaCustomer == customerName)
            {
                string currentStatus = order.Status?.Trim() ?? "";
                if (currentStatus == "Dibatalkan" || currentStatus == "Dibatalkan oleh Teknisi")
                {
                    _context.WaitingResponOrders.Remove(order);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("WaitingOrder");
        }
        //Pembayaran
        [HttpGet]
        public async Task<IActionResult> DaftarTagihan()
        {
                        var customerName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(customerName)) return RedirectToAction("Login", "Account");

             var pendingBills = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .Where(s => s.WaitingResponOrder.NamaCustomer == customerName &&
                            s.WaitingResponOrder.Status == "Menunggu Pembayaran")
                .OrderByDescending(s => s.TimeStop)
                .ToListAsync();

          if (pendingBills.Count == 0)
            {
                return View("EmptyBill");
            }
            if (pendingBills.Count == 1)
            {
                var nomorService = pendingBills.First().WaitingResponOrder.NomorService;
                return RedirectToAction("StatusPembayaran", new { nomor = nomorService });
            }
            return View(pendingBills);
        }
        [HttpGet]
        public async Task<IActionResult> StatusPembayaran(string? nomor)
        {
             var customerName = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(customerName))
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .Where(s => s.WaitingResponOrder.NamaCustomer == customerName);

            if (!string.IsNullOrEmpty(nomor))
            {

                query = query.Where(s => s.WaitingResponOrder.NomorService == nomor);
            }
            else
            {

                query = query.Where(s => s.WaitingResponOrder.Status == "Menunggu Pembayaran");
            }

            var unpaidBill = await query
                .OrderByDescending(s => s.TimeStop)
                .FirstOrDefaultAsync();

            if (unpaidBill == null)
            {

                return View("EmptyBill");
            }

            return View(unpaidBill);
        }
        //metode pembayaran
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PilihMetodeCash(int id)
        {
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceLog == null) return NotFound();

            serviceLog.StatusPembayaran = "Cash";
            serviceLog.WaitingResponOrder.Status = "Menunggu Konfirmasi Cash"; // Status ini yang dibaca di ValidasiPembayaran Teknisi

            await _context.SaveChangesAsync();

            return RedirectToAction("MenungguKonfirmasiTeknisi", new { id = serviceLog.Id });
        }

        public async Task<IActionResult> MenungguKonfirmasiTeknisi(int id)
        {
            // cari data di tabel ServiceLogs (Tabel Aktif)
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceLog == null)
            {
                var username = HttpContext.Session.GetString("Username");

                var arsip = await _context.SelesaiServices
                    .Where(s => s.NamaCustomer == username)
                    .OrderByDescending(s => s.WaktuSelesai) // Ambil yang paling baru
                    .FirstOrDefaultAsync();

                if (arsip != null)
                {
                    return RedirectToAction("StrukLunas", new { nomor = arsip.NomorService });
                }

                return RedirectToAction("DaftarTagihan");
            }

            if (serviceLog.WaitingResponOrder.Status == "Selesai")
            {
                return RedirectToAction("StrukLunas", new { nomor = serviceLog.WaitingResponOrder.NomorService });
            }

            return View(serviceLog);
        }
        //riwayat selesai service
        [HttpGet]
        public async Task<IActionResult> RiwayatService()
        {
                     var customerName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(customerName)) return RedirectToAction("Login", "Account");

            // 2. Ambil data dari tabel ARSIP (SelesaiServices)
            var history = await _context.SelesaiServices
                .Where(s => s.NamaCustomer == customerName)
                .OrderByDescending(s => s.WaktuSelesai) // Paling baru di atas
                .ToListAsync();

            return View(history);
        }
        [HttpGet]
        public async Task<IActionResult> StrukLunas(string nomor)
        {
            // Cari data di tabel ARSIP (SelesaiServices) berdasarkan Nomor Service
            var arsip = await _context.SelesaiServices
                .FirstOrDefaultAsync(s => s.NomorService == nomor);
                if (arsip == null)
            {
                return RedirectToAction("RiwayatService");
            }

            return View(arsip);
        }
    }
}
   