using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Models.Repositories;
using System;
using System.Linq;

namespace SAS.Controllers
{
    public class PrincipalController : Controller
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Notice> _noticeRepo;

        public PrincipalController(
            IRepository<Student> studentRepo,
            IRepository<User> userRepo,
            IRepository<Notice> noticeRepo)
        {
            _studentRepo = studentRepo;
            _userRepo = userRepo;
            _noticeRepo = noticeRepo;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            var students = _studentRepo.GetAll();
            var grouped = students
                .GroupBy(s => s.Std)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.GroupBy(s => s.Div.ToUpper())
                          .ToDictionary(d => d.Key, d => d.ToList())
                );

            var teachers = _userRepo.GetAll()
                .Where(u => u.Role == UserRole.Teacher)
                .ToList();

            var notices = _noticeRepo.GetAll().OrderByDescending(n => n.Date).ToList();

            ViewBag.Profile = GetCurrentUser();
            ViewBag.Teachers = teachers;
            ViewBag.GroupedStudents = grouped;
            ViewBag.Notices = notices;

            return View(students);
        }

        // ==== NOTICE CRUD ====

        [HttpGet]
        public IActionResult CreateNotice()
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");
            return View(new Notice { Date = DateTime.Today });
        }

        [HttpPost]
        public IActionResult CreateNotice(Notice notice)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            if (!ModelState.IsValid) return View(notice);

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            notice.UserId = currentUser.Id;
            _noticeRepo.Add(notice);

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult EditNotice(int id)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            var notice = _noticeRepo.GetAll().FirstOrDefault(n => n.NoticeId == id);
            if (notice == null) return NotFound();

            return View(notice);
        }

        [HttpPost]
        public IActionResult EditNotice(int id, Notice updatedNotice)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            if (!ModelState.IsValid) return View(updatedNotice);

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            updatedNotice.UserId = currentUser.Id;
            var success = _noticeRepo.Update(currentUser.Email, updatedNotice);

            if (!success) return NotFound();

            return RedirectToAction("Dashboard");
        }

        public IActionResult DeleteNotice(int id)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            var success = (_noticeRepo as SQLNoticeRepository)?.DeleteById(id) ?? false;

            return RedirectToAction("Dashboard");
        }

        // ==== STUDENT CRUD (unchanged) ====

        public IActionResult CreateStudent()
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");
            return View();
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            if (!ModelState.IsValid) return View(student);
            _studentRepo.Add(student);
            return RedirectToAction("Dashboard");
        }

        public IActionResult EditStudent(string email)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            var student = _studentRepo.GetByEmail(email);
            if (student == null)
                return NotFound();
            else
                return View(student);
        }

        [HttpPost]
        public IActionResult EditStudent(string email, Student updated)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            if (!ModelState.IsValid) return View(updated);
            _studentRepo.Update(email, updated);
            return RedirectToAction("Dashboard");
        }

        public IActionResult DeleteStudent(string email)
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

            _studentRepo.Delete(email);
            return RedirectToAction("Dashboard");
        }

        public IActionResult Profile()
        {
            if (!IsAuthorized("principal")) return RedirectToAction("Login", "User");

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
