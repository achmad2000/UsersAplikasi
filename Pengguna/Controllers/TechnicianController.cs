using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Pengguna.Hubs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pengguna.Data;
using Pengguna.Models;

namespace Pengguna.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<JobOrderHub> _hubContext;
        private readonly IWebHostEnvironment _environment;

        public TechnicianController(ApplicationDbContext context, IHubContext<JobOrderHub> hubContext, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            _hubContext = hubContext;
        }
        public IActionResult ReportOrder()
        {
            if (HttpContext.Session.GetString("UserRole") != "Technician")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["ActivePage"] = "ReportOrder";
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

            // Simpan perubahan
            _context.Users.Update(user);
            _context.SaveChanges();

            // Perbarui session
            HttpContext.Session.SetString("Username", user.Username);

            TempData["Success"] = "Profil berhasil diperbarui!";
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath ?? "~/images/profile/default-profile.png");
            return RedirectToAction("Profile");

        }
        public IActionResult JobList()
        {
            var waitingOrders = _context.WaitingResponOrders
                                        .Where(o => o.Status == "Menunggu Teknisi" || o.Status == "Menunggu Persetujuan Cancel")
                                        .ToList();

            ViewData["ActivePage"] = "JobList";
            return View(waitingOrders);
        }

        // Take Job
        [HttpPost]
        public async Task<IActionResult> TakeJob(int id)
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            var technicianName = HttpContext.Session.GetString("Username");
            order.Status = "Diterima Teknisi";
            order.IsTaken = true;
            order.NamaTeknisi = technicianName ?? "Technician"; // Fallback jika session kosong
            var serviceLog = new ServiceLog
            {
                WaitingResponOrderId = order.Id, // Link ke order aslinya
                TotalHarga = 0,
                StatusPembayaran = "Belum Lunas",
                TimeStart = null, // Belum dimulai
                TimeStop = null
            };
            _context.ServiceLogs.Add(serviceLog);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Customer_{order.NamaCustomer}")
                             .SendAsync("UpdateReceived");

            return RedirectToAction("JobList");
        }
        public async Task<IActionResult> ActiveJobs()
        {
            // Ambil nama teknisi yang sedang login
            var technicianName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(technicianName))
            {
                return RedirectToAction("Login", "Account"); // Harus login
            }

            // [BARU] Ambil data dari ServiceLogs (bukan WaitingResponOrders)
            // yang ditugaskan ke teknisi ini DAN belum selesai (TimeStop masih null)
            var myActiveJobs = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder) // Join ke order asli untuk ambil info (Alamat, Nama, dll)
                .Where(s => s.WaitingResponOrder.NamaTeknisi == technicianName &&
                            s.TimeStop == null) // Hanya yang aktif
                .OrderBy(s => s.WaitingResponOrder.TanggalOrder)
                .ToListAsync();

            ViewData["ActivePage"] = "ActiveJobs"; // Untuk di layout
            return View(myActiveJobs);
            // Anda perlu membuat file View baru di /Views/Technician/ActiveJobs.cshtml
        }
        [HttpPost]
        public async Task<IActionResult> ApproveCancel(int id)
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Status = "Dibatalkan oleh Teknisi";
                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group($"Customer_{order.NamaCustomer}")
                                 .SendAsync("UpdateReceived");
            }
            return RedirectToAction("JobList");
        }

        [HttpPost]
        public async Task<IActionResult> RejectCancel(int id)
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Status = "Aktif (Lanjut Service)";
                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group($"Customer_{order.NamaCustomer}")
                                 .SendAsync("UpdateReceived");
            }
            return RedirectToAction("JobList");
        }
        // Proses Services
        public async Task<IActionResult> ProcessService(int id) // 'id' ini adalah ID dari ServiceLog
        {
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder) // Info Customer, Alamat
                .Include(s => s.ServiceLogDetails)  // Daftar item yg sudah ditagih
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceLog == null)
            {
                return NotFound();
            }
            var allServiceItems = await _context.ServiceItems
                                                .OrderBy(i => i.NamaItem)
                                                .ToListAsync();
            var viewModel = new ProcessServiceViewModel
            {
                ServiceLog = serviceLog,

                // 1. Data untuk Dropdown 1 (Jenis Service)
                //    Ambil daftar unik "JenisService" dari item yang ada
                JenisServiceList = allServiceItems
             .Select(i => i.JenisService)
             .Distinct()
             .OrderBy(jenis => jenis)
             .Select(jenis => new SelectListItem { Value = jenis, Text = jenis }),

                // 2. Data untuk Dropdown 2 (Tindakan)
                //    Kirim SEMUA item sebagai JSON agar bisa difilter oleh JavaScript
                AllItemsJson = JsonSerializer.Serialize(allServiceItems.Select(item => new
                {
                    id = item.Id,
                    nama = $"{item.NamaItem} (Rp {item.Harga:N0})",
                    jenis = item.JenisService
                })),

                NewItemQuantity = 1
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> StartService(int id)
        {
            var serviceLog = await _context.ServiceLogs.FindAsync(id);
            if (serviceLog != null && serviceLog.TimeStart == null)
            {
                serviceLog.TimeStart = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ProcessService", new { id = id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItemToService(int serviceLogId, int newItemId, int newItemQuantity)
        {
            var itemToAdd = await _context.ServiceItems.FindAsync(newItemId);
            if (itemToAdd == null || newItemQuantity < 1)
            {
                return RedirectToAction("ProcessService", new { id = serviceLogId });
            }

            var serviceLog = await _context.ServiceLogs.FindAsync(serviceLogId);
            if (serviceLog == null)
            {
                return NotFound();
            }
            var logDetail = new ServiceLogDetail
            {
                ServiceLogId = serviceLogId,
                NamaBarang = itemToAdd.NamaItem,
                HargaSatuan = itemToAdd.Harga,
                Jumlah = newItemQuantity
            };
            _context.ServiceLogDetails.Add(logDetail);
            await _context.SaveChangesAsync();
            serviceLog.TotalHarga = await _context.ServiceLogDetails
                .Where(d => d.ServiceLogId == serviceLogId)
                .SumAsync(d => d.Jumlah * d.HargaSatuan);

            await _context.SaveChangesAsync();
            return RedirectToAction("ProcessService", new { id = serviceLogId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItemFromService(int id)
        {
            var detailItem = await _context.ServiceLogDetails.FindAsync(id);
            if (detailItem == null)
            {
                return NotFound();
            }

            int serviceLogId = detailItem.ServiceLogId;
            _context.ServiceLogDetails.Remove(detailItem);
            await _context.SaveChangesAsync();
            var serviceLog = await _context.ServiceLogs.FindAsync(serviceLogId);
            if (serviceLog != null)
            {
                serviceLog.TotalHarga = await _context.ServiceLogDetails
                    .Where(d => d.ServiceLogId == serviceLogId)
                    .SumAsync(d => d.Jumlah * d.HargaSatuan);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ProcessService", new { id = serviceLogId });
        }
        // Di dalam TechnicianController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopService(int id, string statusPembayaran) // 'id' adalah ServiceLogId
        {
            var serviceLog = await _context.ServiceLogs.FindAsync(id);

            if (serviceLog == null)
            {
                return NotFound();
            }

            // 1. Catat waktu selesai (HANYA jika belum selesai)
            if (serviceLog.TimeStop == null)
            {
                serviceLog.TimeStop = DateTime.Now;
            }

            // 2. Update status pembayaran (Lunas / Belum Lunas)
            serviceLog.StatusPembayaran = statusPembayaran;

            // 3. Simpan perubahan ke DB
            await _context.SaveChangesAsync();
            return RedirectToAction("ActiveJobs");
        }
    }

}

