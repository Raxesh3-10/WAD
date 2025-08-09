using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Models.Repositories;
using System.Linq;

namespace SAS.Controllers
{
    public class TrusteeController : Controller
    {
        private readonly IRepository<User> _userRepo;

        public TrusteeController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");

            var allUsers = _userRepo.GetAll();
            var teachers = allUsers.Where(u => u.Role == UserRole.Teacher).ToList();
            var principals = allUsers.Where(u => u.Role == UserRole.Principal).ToList();

            ViewBag.Teachers = teachers;
            ViewBag.Principals = principals;
            ViewBag.Profile = GetCurrentUser();
            return View();
        }

        public IActionResult Profile()
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");

            var user = GetCurrentUser();
            return View(user);
        }

        private bool IsAuthorized(string role) =>
            HttpContext.Session.GetString("UserRole") == role;

        private User? GetCurrentUser()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            return _userRepo.GetByEmail(email ?? "");
        }
    }
}
