using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Pengguna.Data; 
using Pengguna.Models; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pengguna.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // regis
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string Username, string Email, string PhoneNumber, string PasswordHash, string Role)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(PasswordHash))
            {
                ViewBag.Error = "Nama Lengkap, Email, dan Password wajib diisi!";
                ViewData["Username"] = Username;
                ViewData["Email"] = Email;
                ViewData["PhoneNumber"] = PhoneNumber;
                return View();
            }
            string cleanEmail = Email.Trim().ToUpper();
            var existingUser = _context.Users.FirstOrDefault(u => u.Email.ToUpper() == cleanEmail);

            if (existingUser != null)
            {
                ViewBag.Error = "Email sudah digunakan!";
                ViewData["Username"] = Username;
                ViewData["Email"] = Email;
                ViewData["PhoneNumber"] = PhoneNumber;
                return View();
            }

            var newUser = new UserModel
            {
                Username = Username.Trim(),
                Email = Email.Trim(),
                PhoneNumber = PhoneNumber.Trim(), 
                PasswordHash = ComputeSha256Hash(PasswordHash),
                Role = string.IsNullOrEmpty(Role) ? "Customer" : Role
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["Message"] = "Registrasi berhasil! Silakan login.";
            return RedirectToAction("Login");
        }

        // login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserRole") != null)
                return RedirectToAction("RedirectByRole");

            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Email, password, dan Role wajib diisi!";
                return View();
            }

            string cleanEmail = email.Trim().ToUpper();
            string cleanPassword = password.Trim();
            string cleanRole = role.Trim().ToUpper();
            // Hitung hash
            string hashedPassword = ComputeSha256Hash(cleanPassword);
            var user = _context.Users.FirstOrDefault(u =>
                u.Email.ToUpper() == cleanEmail &&
                u.PasswordHash == hashedPassword &&
                u.Role.ToUpper() == cleanRole
            );
            Console.WriteLine($"Mencoba login: Email: {cleanEmail}, Role: {cleanRole}");

            if (user == null)
            {
                ViewBag.Error = "Email, password, atau peran yang dipilih salah!";
                return View();
            }
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);
            string imagePath = user.ProfileImagePath ?? "/images/profile/default-profile.png";
            HttpContext.Session.SetString("ProfileImagePath", imagePath);
            // Redirect role
            return RedirectToAction("RedirectByRole");
        }

        // Helper
        public IActionResult RedirectByRole()
        {
            string role = HttpContext.Session.GetString("UserRole");

            return role switch
            {
                "Technician" => RedirectToAction("ReportOrder", "Technician"),
                "Customer" => RedirectToAction("OrderCustomer", "Customer"),
                _ => RedirectToAction("Login")
            };
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}