using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Models.Repositories;
using SAS.Services;
using System;

namespace SAS.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly MailService _mailService;

        public UserController(IRepository<User> userRepo, MailService mailService)
        {
            _userRepo = userRepo;
            _mailService = mailService;
        }

        // ========== LOGIN ==========

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            var user = _userRepo.GetByEmail(email);

            if (user == null || !user.VerifyPassword(password))
                return RedirectToAction("Login"); // Skip error

            if (!user.HasRole(Enum.Parse<UserRole>(role, true)))
                return RedirectToAction("Login"); // Skip error

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", role.ToLower());
            HttpContext.Session.SetString("UserName", user.Name);

            return role.ToLower() switch
            {
                "teacher" => RedirectToAction("Dashboard", "Teacher"),
                "principal" => RedirectToAction("Dashboard", "Principal"),
                "trustee" => RedirectToAction("Dashboard", "Trustee"),
                _ => RedirectToAction("Login")
            };
        }


        // ========== SIGNUP + OTP ==========

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(User user, string otp)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (!OtpHelper.VerifyOtp(HttpContext, user.Email, otp))
            {
                ViewBag.Error = "Invalid or expired OTP.";
                return View(user);
            }

            var role = user.Role.ToString().ToLower();
            if (role == "teacher" || role == "principal")
            {
                // Read extra fields manually
                 int.TryParse(Request.Form["Std"], out int std);
                var div = Request.Form["Div"];
                var salary = Request.Form["Salary"];

                // Attach to user object
                user.Std = std;
                user.Div = div;
                if (decimal.TryParse(salary, out decimal sal))
                    user.Salary = sal;
            }
            _userRepo.Add(user);
            OtpHelper.ClearOtp(HttpContext);
            return RedirectToAction("Login");
        }


        [HttpPost]
        public IActionResult SendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Email required" });

            string otp = OtpHelper.GenerateOtp();
            OtpHelper.StoreOtp(HttpContext, email, otp);
            _mailService.SendOtp(email, otp);
            return Json(new { success = true, message = "OTP sent" });
        }

        // ========== SESSION ==========

        [HttpGet]
        public IActionResult CheckSession()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");
            if (email == null || role == null)
                return Json(new { success = false });

            return Json(new { success = true, email, role });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ========== PROFILE ==========

        [HttpGet]
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
                return RedirectToAction("Login");

            var user = _userRepo.GetByEmail(email);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateProfile(User updated, string otp, string ConfirmPassword)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
                return RedirectToAction("Login");
            if (!OtpHelper.VerifyOtp(HttpContext, email, otp))
            {
                ViewBag.Error = "Invalid OTP.";
                return View("Profile", updated);
            }
            if (updated.Password != ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View("Profile", updated);
            }
            _userRepo.Update(email, updated);
            OtpHelper.ClearOtp(HttpContext);

            return RedirectToAction("Login");
        }


        // ========== BUG REPORT ==========

        [HttpGet]
        public IActionResult ReportBug()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReportBug(BugReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = "Invalid input.";
                ViewBag.StatusClass = "danger";
                return View(model);
            }

            try
            {
                _mailService.ReportBug(model.Name, model.Email, model.Message);
                ViewBag.Status = "Bug report sent successfully.";
                ViewBag.StatusClass = "success";
            }
            catch (Exception ex)
            {
                ViewBag.Status = "Failed to send bug report. Please try again.";
                ViewBag.StatusClass = "danger";
            }

            return View(model);
        }

    }
}
