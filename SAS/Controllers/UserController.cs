using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.Services;
using SAS.ViewModels;
using AutoMapper;
using System;

namespace SAS.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IUserDetailsRepository _detailsRepo;
        private readonly MailService _mailService;
        private readonly IMapper _mapper;

        public UserController(
            IRepository<User> userRepo,
            IUserDetailsRepository detailsRepo,
            MailService mailService,
            IMapper mapper)
        {
            _userRepo = userRepo;
            _detailsRepo = detailsRepo;
            _mailService = mailService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Login()
        {
            Logout();
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, string role)
        {
            var user = _userRepo.GetByEmail(email);
            if (user == null || !user.VerifyPassword(password))
            {
                ViewBag.Message = "Invalid credentials";
                return View();
            }

            if (!user.HasRole(Enum.Parse<UserRole>(role, true)))
            {
                ViewBag.Message = "Role mismatch";
                return View();
            }
            HttpContext.Session.SetString("UserId",user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", role.ToLower());
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Dashboard", role, new { area = "" });
        }

        [HttpGet]
        public IActionResult Signup()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public IActionResult Signup(UserViewModel userVm, string otp)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid input.";
                return View(userVm);
            }

            if (!OtpHelper.VerifyOtp(HttpContext, userVm.Email, otp))
            {
                ViewBag.Message = "Invalid or expired OTP";
                return View(userVm);
            }

            var user = _mapper.Map<User>(userVm);
            _userRepo.Add(user);
            if(userVm.Role!="trustee")
                _detailsRepo.CreateEmptyDetails(user.Id);

            OtpHelper.ClearOtp(HttpContext);

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult SendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { message = "Email required" });

            string otp = OtpHelper.GenerateOtp();
            OtpHelper.StoreOtp(HttpContext, email, otp);
            _mailService.SendOtp(email, otp);

            return Json(new { message = "OTP sent" });
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login");

            var user = _userRepo.GetByEmail(email);
            if (user == null) return NotFound();

            var userVm = _mapper.Map<UserViewModel>(user);
            return View(userVm);
        }

        [HttpPost]
        public IActionResult UpdateProfile(UserViewModel updatedVm, string otp, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login");

            if (!OtpHelper.VerifyOtp(HttpContext, email, otp))
            {
                ViewBag.Message = "Invalid OTP";
                return View("Profile", updatedVm);
            }

            if (updatedVm.Password != confirmPassword)
            {
                ViewBag.Message = "Passwords do not match";
                return View("Profile", updatedVm);
            }

            var updatedUser = _mapper.Map<User>(updatedVm);
            _userRepo.Update(email, updatedUser);
            OtpHelper.ClearOtp(HttpContext);

            ViewBag.Message = "Profile updated successfully";
            return View("Profile", updatedVm);
        }

        [HttpPost]
        public IActionResult DeleteAccount(Guid id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return Unauthorized();

            var user = _userRepo.GetByEmail(email);
            if (user == null) return NotFound();

            _userRepo.Delete(user.Email);
            HttpContext.Session.Clear();

            return Ok();
        }
        [HttpGet]
        public IActionResult ReportBug()
        {
            var model = new ReportBugViewModel
            {
                Name = HttpContext.Session.GetString("UserName") ?? "",
                Email = HttpContext.Session.GetString("UserEmail") ?? ""
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult ReportBug(ReportBugViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.StatusMessage = "❌ Invalid input. Please try again.";
                return View(model);
            }

            try
            {
                _mailService.ReportBug(model.Name, model.Email, model.Message);
                model.StatusMessage = "✅ Bug report sent successfully!";
            }
            catch (Exception)
            {
                model.StatusMessage = "⚠️ Failed to send bug report. Please try again later.";
            }

            return View(model);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}