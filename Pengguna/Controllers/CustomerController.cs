using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Pengguna.Models;
using Pengguna.Data;
using System.IO;
using System.Linq;

namespace Pengguna.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context; 
        private readonly IWebHostEnvironment _environment;

        public CustomerController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        //public IActionResult ReportOrder()
        //{
        //    if (HttpContext.Session.GetString("UserRole") != "Technician")
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    ViewData["ActivePage"] = "ReportOrder";
        //    return View();
        //}
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

        [HttpPost]
        public IActionResult SubmitOrder(WaitingResponOrder model)
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
                return RedirectToAction("WaitingOrder");
            }
            return View("Order", model);
        }

        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            var order = _context.WaitingResponOrders.FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();
            if (order.Status == "Menunggu Teknisi")
            {
                order.Status = "Dibatalkan";
                _context.SaveChanges();
                TempData["Message"] = "Pesanan berhasil dibatalkan.";
            }
            else if (order.Status == "Diterima Teknisi" || order.Status == "Aktif (Lanjut Service)")
            {
                order.Status = "Menunggu Persetujuan Cancel";
                _context.SaveChanges();
                TempData["Message"] = "Permintaan pembatalan telah dikirim ke teknisi.";
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
    }
}
   