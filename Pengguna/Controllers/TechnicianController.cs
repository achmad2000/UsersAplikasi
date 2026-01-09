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
        public IActionResult OrderCustomer()
        {
            ViewData["ActivePage"] = "OrderCustomer";
            return View();
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
            order.NamaTeknisi = technicianName ?? "Technician"; 
            var serviceLog = new ServiceLog
            {
                WaitingResponOrderId = order.Id, 
                TotalHarga = 0,
                StatusPembayaran = "Belum Lunas",
                TimeStart = null, 
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
            var technicianName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(technicianName))
            {
                return RedirectToAction("Login", "Account"); // Harus login
            }

            var myActiveJobs = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder) 
                .Where(s => s.WaitingResponOrder.NamaTeknisi == technicianName &&
                            s.TimeStop == null) // Hanya yang aktif
                .OrderBy(s => s.WaitingResponOrder.TanggalOrder)
                .ToListAsync();

            ViewData["ActivePage"] = "ActiveJobs"; // Untuk di layout
            return View(myActiveJobs);
        }
        //Mekanisme cancel
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
                JenisServiceList = allServiceItems
             .Select(i => i.JenisService)
             .Distinct()
             .OrderBy(jenis => jenis)
             .Select(jenis => new SelectListItem { Value = jenis, Text = jenis }),

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
        //penambahan item
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
                JenisService = itemToAdd.JenisService,
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
        //mekanisme pembayaran
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopService(int id)
        {
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceLog == null) return NotFound();
            if (serviceLog.TimeStop == null)
                serviceLog.TimeStop = DateTime.Now;
            serviceLog.WaitingResponOrder.Status = "Menunggu Pembayaran";

            serviceLog.StatusPembayaran = "Belum Dipilih";

            await _context.SaveChangesAsync();

            return RedirectToAction("ValidasiPembayaran");
        }

        public async Task<IActionResult> ValidasiPembayaran()
        {
            var technicianName = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(technicianName)) return RedirectToAction("Login", "Account");

            var pendingValidations = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .Where(s => s.WaitingResponOrder.NamaTeknisi == technicianName &&
                            (
                                s.WaitingResponOrder.Status == "Menunggu Pembayaran" ||
                                s.WaitingResponOrder.Status == "Menunggu Konfirmasi Cash" ||                         
                                s.WaitingResponOrder.Status == "Menunggu Verifikasi Transfer"
                            ))
                .OrderByDescending(s => s.TimeStop)
                .ToListAsync();

            ViewData["ActivePage"] = "ValidasiPembayaran";
            return View(pendingValidations);
        }
        // Acc pembayaran
        //[HttpPost]
        //public async Task<IActionResult> ConfirmPayment(int id)
        //{
        //    // 1. Ambil Data Log & Order
        //    var serviceLog = await _context.ServiceLogs
        //        .Include(s => s.WaitingResponOrder)
        //        .Include(s => s.ServiceLogDetails)
        //        .FirstOrDefaultAsync(s => s.Id == id);

        //    if (serviceLog == null || serviceLog.WaitingResponOrder == null)
        //    {
        //        return RedirectToAction("ValidasiPembayaran");
        //    }

        //    var orderLama = serviceLog.WaitingResponOrder;
        //    string namaCustomerSafe = (orderLama.NamaCustomer ?? "").Trim();
        //    string targetNomorService = orderLama.NomorService;

        //    // Susun Rincian Item
        //    string rincianItem = orderLama.ItemService;
        //    if (serviceLog.ServiceLogDetails != null && serviceLog.ServiceLogDetails.Any())
        //    {
        //        var listBarang = serviceLog.ServiceLogDetails
        //            .Select(d => $"{d.JenisService}|{d.NamaBarang}")
        //            .ToArray();
        //        rincianItem = string.Join(",", listBarang);
        //    }

        //    // 2. Buat Arsip Baru
        //    var arsipBaru = new SelesaiService
        //    {
        //        NomorService = orderLama.NomorService,
        //        NamaCustomer = namaCustomerSafe, // Pakai yang safe
        //        ItemService = rincianItem,
        //        JadwalService = orderLama.JadwalService,
        //        NoWA = orderLama.NoWA,
        //        Alamat = orderLama.Alamat,
        //        DeskripsiProblem = orderLama.DeskripsiProblem,
        //        TanggalOrder = orderLama.TanggalOrder,
        //        StatusPembayaran = serviceLog.StatusPembayaran ?? "Lunas (Cash)",
        //        TechnicianId = orderLama.TechnicianId,
        //        NamaTeknisi = orderLama.NamaTeknisi,
        //        TotalBiaya = serviceLog.TotalHarga,
        //        WaktuSelesai = DateTime.Now
        //    };

        //    _context.SelesaiServices.Add(arsipBaru);
        //    var logsToDelete = _context.ServiceLogs.Where(s => s.WaitingResponOrder.NomorService == targetNomorService).ToList();
        //    var logIds = logsToDelete.Select(l => l.Id).ToList();
        //    var detailsToDelete = _context.ServiceLogDetails.Where(d => logIds.Contains(d.ServiceLogId)).ToList();
        //    var orderToDelete = _context.WaitingResponOrders.Where(o => o.NomorService == targetNomorService).ToList();

        //    if (detailsToDelete.Any()) _context.ServiceLogDetails.RemoveRange(detailsToDelete);
        //    if (logsToDelete.Any()) _context.ServiceLogs.RemoveRange(logsToDelete);
        //    if (orderToDelete.Any()) _context.WaitingResponOrders.RemoveRange(orderToDelete);
        //    await _context.SaveChangesAsync();
        //    if (_hubContext == null)
        //    {
        //        Console.WriteLine("BAHAYA: _hubContext bernilai NULL. Cek Constructor Controller!");
        //    }
        //    else
        //    {

        //        string groupTarget = $"Customer_{namaCustomerSafe.ToLower()}";

        //        await _hubContext.Clients.Group(groupTarget)
        //            .SendAsync("RedirectToRating", arsipBaru.Id);

        //        // await _hubContext.Clients.All.SendAsync("RedirectToRating", arsipBaru.Id);
        //    }

        //    return RedirectToAction("ValidasiPembayaran");
        //}
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            // 1. Ambil Data Log & Order dengan Include lengkap
            var serviceLog = await _context.ServiceLogs
                .Include(s => s.WaitingResponOrder)
                .Include(s => s.ServiceLogDetails)
                .FirstOrDefaultAsync(s => s.Id == id);

            // Cek null safety agar tidak error
            if (serviceLog == null || serviceLog.WaitingResponOrder == null)
            {
                return RedirectToAction("ValidasiPembayaran");
            }

            var orderLama = serviceLog.WaitingResponOrder;
            string namaCustomerSafe = (orderLama.NamaCustomer ?? "").Trim();
            string targetNomorService = orderLama.NomorService;

            // Susun Rincian Item untuk arsip
            string rincianItem = orderLama.ItemService;
            if (serviceLog.ServiceLogDetails != null && serviceLog.ServiceLogDetails.Any())
            {
                var listBarang = serviceLog.ServiceLogDetails
                    .Select(d => $"{d.JenisService}|{d.NamaBarang}")
                    .ToArray();
                rincianItem = string.Join(",", listBarang);
            }

            // 2. Buat Arsip Baru di Tabel SelesaiService
            var arsipBaru = new SelesaiService
            {
                NomorService = orderLama.NomorService,
                NamaCustomer = namaCustomerSafe,
                ItemService = rincianItem,
                JadwalService = orderLama.JadwalService,
                NoWA = orderLama.NoWA,
                Alamat = orderLama.Alamat,
                DeskripsiProblem = orderLama.DeskripsiProblem,
                TanggalOrder = orderLama.TanggalOrder,
                StatusPembayaran = serviceLog.StatusPembayaran ?? "Lunas (Cash)",
                TechnicianId = orderLama.TechnicianId,
                NamaTeknisi = orderLama.NamaTeknisi,
                TotalBiaya = serviceLog.TotalHarga,
                WaktuSelesai = DateTime.Now,

                // --- POIN PENTING: Set Status Rating ke FALSE ---
                IsRated = false
            };

            _context.SelesaiServices.Add(arsipBaru);

            // 3. Hapus Data Lama (Bersih-bersih)
            var logsToDelete = _context.ServiceLogs.Where(s => s.WaitingResponOrder.NomorService == targetNomorService).ToList();
            var logIds = logsToDelete.Select(l => l.Id).ToList();
            var detailsToDelete = _context.ServiceLogDetails.Where(d => logIds.Contains(d.ServiceLogId)).ToList();
            var orderToDelete = _context.WaitingResponOrders.Where(o => o.NomorService == targetNomorService).ToList();

            if (detailsToDelete.Any()) _context.ServiceLogDetails.RemoveRange(detailsToDelete);
            if (logsToDelete.Any()) _context.ServiceLogs.RemoveRange(logsToDelete);
            if (orderToDelete.Any()) _context.WaitingResponOrders.RemoveRange(orderToDelete);

            // Simpan Perubahan ke Database
            await _context.SaveChangesAsync();

            // 4. SignalR (Opsional / Backup)
            // Tetap kita pasang sebagai "Trigger Cepat", tapi kalau gagal pun tidak masalah
            // karena Customer nanti akan kena cegat di halaman utama.
            if (_hubContext != null)
            {
                // Broadcast ke semua untuk memastikan notif status terkirim
                await _hubContext.Clients.All.SendAsync("RedirectToRating", arsipBaru.Id);
            }

            return RedirectToAction("ValidasiPembayaran");
        }
        //garansi
        public async Task<IActionResult> GaransiTech()
        {
                  var technicianName = HttpContext.Session.GetString("Username");

                       if (string.IsNullOrEmpty(technicianName)) return RedirectToAction("Login", "Account");

           var jobGaransi = await _context.WaitingResponOrders
                .Where(w => w.NamaTeknisi == technicianName && w.NomorService.StartsWith("G-"))
                .OrderByDescending(w => w.TanggalOrder)
                .ToListAsync();

            ViewData["ActivePage"] = "Garansi";

            return View(jobGaransi);
        }
         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeJobGaransiNow(int id)
        {
            // 1. Cari Data Order
            var order = await _context.WaitingResponOrders.FindAsync(id);
            if (order == null) return NotFound();

            // 2. Cek apakah sudah ada di ServiceLog (Mencegah duplikat)
            var cekLog = await _context.ServiceLogs
                .AnyAsync(s => s.WaitingResponOrderId == id);

            if (cekLog)
            {
                TempData["Error"] = "Pekerjaan ini sudah ada di Active Job!";
                return RedirectToAction("ActiveJobs");
            }

                     var newLog = new ServiceLog
            {
                WaitingResponOrderId = id,
                TimeStart = DateTime.Now,
                TotalHarga = 0,
                StatusPembayaran = "Claim Garansi"
            };

            _context.ServiceLogs.Add(newLog);
            order.Status = "Sedang Dikerjakan";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Job Garansi dimulai! Silakan cek Active Job.";
            return RedirectToAction("ActiveJobs");
        }

        // ---------------------------------------------------------
        // PERBAIKAN: SCHEDULE JOB
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleJobGaransi(int id, DateTime jadwalPengerjaan)
        {
            var order = await _context.WaitingResponOrders.FindAsync(id);
            if (order == null) return NotFound();

            if (jadwalPengerjaan < DateTime.Now)
            {
                TempData["Error"] = "Jadwal tidak boleh di waktu lampau!";
                return RedirectToAction("Garansi");
            }

            // HAPUS property yang error di sini juga
            var newLog = new ServiceLog
            {
                WaitingResponOrderId = id,
                TimeStart = jadwalPengerjaan,
                TotalHarga = 0,
                StatusPembayaran = "Claim Garansi"
            };

            _context.ServiceLogs.Add(newLog);
            order.Status = "Terjadwal";

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Job dijadwalkan pada {jadwalPengerjaan.ToString("dd MMM HH:mm")}. Cek Active Job.";
            return RedirectToAction("ActiveJobs");
        }
    }
}

