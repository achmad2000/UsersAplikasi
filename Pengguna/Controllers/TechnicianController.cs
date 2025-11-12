using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Pengguna.Models;
using Pengguna.Data;
using System.IO;
using System.Linq;

namespace Pengguna.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly ApplicationDbContext _context; 
        private readonly IWebHostEnvironment _environment;

        public TechnicianController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
            // [FIX] Tampilkan job yang 'Menunggu Teknisi' ATAU 'Menunggu Persetujuan Cancel'
            // Ini agar logik di view (joblist.cshtml) bisa berfungsi
            var waitingOrders = _context.WaitingResponOrders
                                        .Where(o => o.Status == "Menunggu Teknisi" || o.Status == "Menunggu Persetujuan Cancel")
                                        .ToList();

            ViewData["ActivePage"] = "JobList";
            return View(waitingOrders);
        }

        // Take Job
        [HttpPost]
        public IActionResult TakeJob(int id)
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                // [LOGIC FIX] Jangan hapus order. Update saja statusnya.
                // Ini adalah kunci sinkronisasi.

                order.Status = "Diterima Teknisi";
                order.IsTaken = true; // Set flag 'IsTaken'

                // [USER REQUEST] Ambil nama teknisi dari user yang login
                order.NamaTeknisi = User.Identity?.Name ?? "Technician";

                // Hapus kode yang memindahkan ke ActiveOrders
                // _context.ActiveOrders.Add(active);
                // _context.WaitingResponOrders.Remove(order);

                _context.SaveChanges();
            }

            return RedirectToAction("JobList");
        }

        [HttpPost]
        // [FIX] Samakan parameter menjadi 'id' agar sesuai dengan form di view
        public IActionResult ApproveCancel(int id)
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Status = "Dibatalkan oleh Teknisi";
                _context.SaveChanges();
            }
            // [FIX] Redirect kembali ke JobList, bukan ActiveOrders
            return RedirectToAction("JobList");
        }

        [HttpPost]
        // [FIX] Samakan parameter menjadi 'id' agar sesuai dengan form di view
        public IActionResult RejectCancel(int id)
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Status = "Aktif (Lanjut Service)"; // Status baru yang lebih jelas
                _context.SaveChanges();
            }
            // [FIX] Redirect kembali ke JobList, bukan ActiveOrders
            return RedirectToAction("JobList");
        }

    }
}
   