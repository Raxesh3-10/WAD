using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Models.Repositories;
using System.Linq;
using System;

namespace SAS.Controllers
{
    public class TrusteeController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Notice> _noticeRepo;

        public TrusteeController(IRepository<User> userRepo, IRepository<Notice> noticeRepo)
        {
            _userRepo = userRepo;
            _noticeRepo = noticeRepo;
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

            // Load all notices
            var notices = _noticeRepo.GetAll();
            ViewBag.Notices = notices;

            return View();
        }

        public IActionResult Profile()
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");

            var user = GetCurrentUser();
            return View(user);
        }

        public IActionResult CreateNotice()
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");
            return View(new Notice { Date = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNotice(Notice notice)
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");

            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                notice.UserId = currentUser.Id;
                _noticeRepo.Add(notice);
                return RedirectToAction(nameof(Dashboard));
            }

            return View(notice);
        }

        public IActionResult EditNotice(int id)
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");

            var notice = _noticeRepo.GetAll().FirstOrDefault(n => n.NoticeId == id);
            if (notice == null) return NotFound();

            return View(notice);
        }

        [HttpPost]
        public IActionResult EditNotice(Notice notice)
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                _noticeRepo.Update(currentUser.Email, notice);
                return RedirectToAction(nameof(Dashboard));
            }

            return View(notice);
        }

        [HttpPost]
        public IActionResult DeleteNotice(int id)
        {
            if (!IsAuthorized("trustee")) return RedirectToAction("Login", "User");

            var deleted = (_noticeRepo as SQLNoticeRepository)?.DeleteById(id) ?? false;
            if (!deleted) return NotFound();

            return RedirectToAction(nameof(Dashboard));
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
