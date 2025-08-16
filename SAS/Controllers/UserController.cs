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
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly MailService _mailService;
        private readonly IMapper _mapper;

        public UserController(IRepository<User> userRepo, MailService mailService, IMapper mapper)
        {
            _userRepo = userRepo;
            _mailService = mailService;
            _mapper = mapper;
        }

        public IActionResult Login([FromBody] LoginViewModel model)
        {
            var user = _userRepo.GetByEmail(model.Email);
            if (user == null || !user.VerifyPassword(model.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            if (!user.HasRole(Enum.Parse<UserRole>(model.Role, true)))
                return Unauthorized(new { message = "Role mismatch" });

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", model.Role.ToLower());
            HttpContext.Session.SetString("UserName", user.Name);

            return Ok(new { message = "Login successful", role = model.Role.ToLower() });
        }

        public IActionResult Signup([FromBody] UserViewModel userVm, string otp)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!OtpHelper.VerifyOtp(HttpContext, userVm.Email, otp))
                return BadRequest(new { message = "Invalid or expired OTP" });

            var user = _mapper.Map<User>(userVm);
            _userRepo.Add(user);
            OtpHelper.ClearOtp(HttpContext);

            return Ok(new { message = "User created successfully", user = _mapper.Map<UserViewModel>(user) });
        }

        public IActionResult SendOtp([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email required" });

            string otp = OtpHelper.GenerateOtp();
            OtpHelper.StoreOtp(HttpContext, email, otp);
            _mailService.SendOtp(email, otp);

            return Ok(new { message = "OTP sent" });
        }

        public IActionResult CheckSession()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (email == null || role == null)
                return Ok(new { success = false });

            return Ok(new { success = true, email, role });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out" });
        }

        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return Unauthorized();

            var user = _userRepo.GetByEmail(email);
            if (user == null) return NotFound();

            var userVm = _mapper.Map<UserViewModel>(user);
            return Ok(userVm);
        }

        public IActionResult UpdateProfile([FromBody] UserViewModel updatedVm, string otp, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return Unauthorized();

            if (!OtpHelper.VerifyOtp(HttpContext, email, otp))
                return BadRequest(new { message = "Invalid OTP" });

            if (updatedVm.Password != confirmPassword)
                return BadRequest(new { message = "Passwords do not match" });

            var updatedUser = _mapper.Map<User>(updatedVm);
            _userRepo.Update(email, updatedUser);
            OtpHelper.ClearOtp(HttpContext);

            return Ok(new { message = "Profile updated successfully", user = updatedVm });
        }

        public IActionResult ReportBug([FromBody] BugReportViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input" });

            try
            {
                _mailService.ReportBug(model.Name, model.Email, model.Message);
                return Ok(new { message = "Bug report sent successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Failed to send bug report" });
            }
        }
    }
}