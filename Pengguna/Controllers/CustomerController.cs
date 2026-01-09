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
        //public IActionResult Index()
        //{
        //    ViewData["ActivePage"] = "Index";
        //    return View();
        //}
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");

            if (!string.IsNullOrEmpty(username))
            {
                var pendingRating = await _context.SelesaiServices
                    .Where(s => s.NamaCustomer == username && s.IsRated == false)
                    .OrderByDescending(s => s.WaktuSelesai) // Prioritaskan yang paling baru selesai
                    .FirstOrDefaultAsync();

                if (pendingRating != null)
                {
                    return RedirectToAction("BeriRating", "Customer", new { id = pendingRating.Id });
                }
            }

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
                .Include(w => w.ServiceLogs)
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
        // 1. HALAMAN DAFTAR GARANSI & STATUS KLAIM
        [HttpGet]
        public async Task<IActionResult> Garansi()
        {
            var customerName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(customerName)) return RedirectToAction("Login", "Account");

            var batasGaransi = DateTime.Now.AddDays(-30);

            var serviceGaransi = await _context.SelesaiServices
                .Where(s => s.NamaCustomer == customerName &&
                            s.WaktuSelesai >= batasGaransi)
                .OrderByDescending(s => s.WaktuSelesai)
                .ToListAsync();
            var listKlaim = await _context.Garansis
                .Where(g => g.NamaCustomer == customerName)
                .OrderByDescending(g => g.TanggalKlaim)
                .ToListAsync();

            ViewBag.ListKlaim = listKlaim;

            return View(serviceGaransi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
            public async Task<IActionResult> AjukanKlaim(string nomorService, string keluhan, string item, IFormFile fotoKerusakan)
        {
            var customerName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(customerName))
            {
                return RedirectToAction("Login", "Account");
            }
            var cekDouble = await _context.Garansis
                .AnyAsync(g => g.NomorServiceRef == nomorService && g.Status == "Menunggu Review");

            if (cekDouble)
            {
                TempData["Error"] = "Pengajuan Anda sebelumnya untuk unit ini masih dalam peninjauan.";
                return RedirectToAction("Garansi");
            }
            string hasilConvertFoto = null;

            if (fotoKerusakan != null && fotoKerusakan.Length > 0)
            {
                if (!fotoKerusakan.ContentType.StartsWith("image/"))
                {
                    TempData["Error"] = "File yang diupload harus berupa gambar.";
                    return RedirectToAction("Garansi");
                }

                using (var memoryStream = new MemoryStream())
                {
                    // Salin file dari upload ke memory sementara
                    await fotoKerusakan.CopyToAsync(memoryStream);

                    // Ubah menjadi array byte
                    var fileBytes = memoryStream.ToArray();

                    // Ubah byte menjadi String Base64
                    string base64String = Convert.ToBase64String(fileBytes);

                    // Gabungkan menjadi format Data URI agar bisa dibaca browser (src="data:image/...")
                    hasilConvertFoto = $"data:{fotoKerusakan.ContentType};base64,{base64String}";
                }
            }
            var klaimBaru = new Garansi
            {
                NomorServiceRef = nomorService,
                NamaCustomer = customerName,
                ItemService = item,
                DeskripsiKeluhan = keluhan,
                Status = "Menunggu Review",
                TanggalKlaim = DateTime.Now,
                BuktiFotoPath = hasilConvertFoto,
                CatatanAdmin = null
            };

            _context.Garansis.Add(klaimBaru);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Pengajuan garansi & bukti foto berhasil dikirim! Mohon tunggu verifikasi admin.";
            return RedirectToAction("Garansi");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PilihMetodeTransfer(int id)
        {
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceLog == null) return NotFound();

            serviceLog.StatusPembayaran = "Transfer";

            serviceLog.WaitingResponOrder.Status = "Menunggu Upload Bukti";

            await _context.SaveChangesAsync();

            return RedirectToAction("UploadBukti", new { id = serviceLog.Id });
        }

        [HttpGet]
        public async Task<IActionResult> UploadBukti(int id)
        {
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceLog == null) return NotFound();
            if (serviceLog.WaitingResponOrder.Status == "Menunggu Verifikasi Transfer" ||
                serviceLog.WaitingResponOrder.Status == "Selesai")
            {
                return RedirectToAction("MenungguKonfirmasiTeknisi", new { id = serviceLog.Id });
            }

            return View(serviceLog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProsesUploadBukti(int id, IFormFile fileBukti)
        {
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceLog == null) return NotFound();

            // Validasi File
            if (fileBukti != null && fileBukti.Length > 0)
            {
                var fileName = $"Bukti_{serviceLog.WaitingResponOrder.NomorService}_{DateTime.Now.Ticks}{Path.GetExtension(fileBukti.FileName)}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileBukti.CopyToAsync(stream);
                }
                serviceLog.BuktiPembayaranPath = "/uploads/" + fileName; // Simpan path relatif
                serviceLog.WaitingResponOrder.Status = "Menunggu Verifikasi Transfer"; // Status baru

                await _context.SaveChangesAsync();
                return RedirectToAction("MenungguKonfirmasiTeknisi", new { id = serviceLog.Id });
            }

            TempData["Error"] = "Harap pilih file bukti transfer.";
            return RedirectToAction("UploadBukti", new { id = id });
        }
        [HttpGet]
        public async Task<IActionResult> BeriRating(int id)
        {
            // BENAR: Ambil dari Arsip
            var dataArsip = await _context.SelesaiServices.FindAsync(id);

            if (dataArsip == null)
            {
                return RedirectToAction("RiwayatService");
            }
            return View(dataArsip);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimpanRating(int id, int jumlahBintang, string ulasan)
        {
            // 1. Cari data arsip berdasarkan ID
            var arsip = await _context.SelesaiServices.FindAsync(id);

            if (arsip == null)
            {
                return RedirectToAction("RiwayatService"); // Atau halaman home
            }

            // 2. Simpan Data Rating (Opsional: Bisa simpan ke tabel Rating terpisah atau update arsip)
            // Disini saya update ke arsip saja sesuai model di atas
            arsip.Rating = jumlahBintang;
            arsip.Ulasan = ulasan;

            // --- POIN PENTING: Update Status Jadi TRUE ---
            arsip.IsRated = true;

            // 3. Simpan perubahan
            _context.SelesaiServices.Update(arsip);
            await _context.SaveChangesAsync();

            // 4. Redirect kembali ke Home/Menu Utama
            TempData["Success"] = "Terima kasih atas penilaian Anda!";
            return RedirectToAction("Index");
        }
        // POST: Simpan Rating
        //[HttpPost]
        //public async Task<IActionResult> SimpanRating(int id, int jumlahBintang, string ulasan)
        //{
        //    // PERBAIKAN: Cari data di tabel ARSIP (SelesaiServices), BUKAN ServiceLogs
        //    var arsip = await _context.SelesaiServices.FindAsync(id);

        //    if (arsip != null)
        //    {
        //        // Simpan ke kolom di tabel SelesaiServices
        //        // Pastikan Model SelesaiService.cs sudah punya properti ini!
        //        arsip.Rating = jumlahBintang;
        //        arsip.Ulasan = ulasan;

        //        _context.SelesaiServices.Update(arsip);
        //        await _context.SaveChangesAsync();
        //    }

        //    TempData["Success"] = "Terima kasih atas penilaian Anda!";

        //    // Redirect ke RiwayatService (sesuai menu di Layout Anda)
        //    return RedirectToAction("RiwayatService");
        //}

    }
}
   