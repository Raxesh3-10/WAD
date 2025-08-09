using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Models.Repositories;
using System.Linq;

namespace SAS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Notice> _noticeRepo; // ✅ Added for notices

        public TeacherController(
            IRepository<Student> studentRepo,
            IRepository<User> userRepo,
            IRepository<Notice> noticeRepo // ✅ Added to constructor
        )
        {
            _studentRepo = studentRepo;
            _userRepo = userRepo;
            _noticeRepo = noticeRepo;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized("teacher")) return RedirectToAction("Login", "User");

            var students = _studentRepo.GetAll();
            var grouped = students
                .GroupBy(s => s.Std)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.GroupBy(s => s.Div.ToUpper())
                          .ToDictionary(d => d.Key, d => d.ToList())
                );

            ViewBag.Profile = GetCurrentUser();
            ViewBag.GroupedStudents = grouped;

            // ✅ Added: All notices for display in dashboard
            var notices = _noticeRepo.GetAll()
                .OrderByDescending(n => n.Date)
                .ToList();
            ViewBag.Notices = notices;

            return View();
        }

        public IActionResult CreateStudent()
        {
            if (!IsAuthorized("teacher")) return RedirectToAction("Login", "User");
            return View();
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            if (!IsAuthorized("teacher")) return RedirectToAction("Login", "User");

            if (!ModelState.IsValid) return View(student);
            _studentRepo.Add(student);
            return RedirectToAction("Dashboard");
        }

        public IActionResult EditStudent(string email)
        {
            if (!IsAuthorized("teacher")) return RedirectToAction("Login", "User");

            var student = _studentRepo.GetByEmail(email);
            if (student == null)
                return NotFound();
            else
                return View(student);
        }

        [HttpPost]
        public IActionResult EditStudent(string email, Student updated)
        {
            if (!IsAuthorized("teacher")) return RedirectToAction("Login", "User");

            if (!ModelState.IsValid) return View(updated);
            _studentRepo.Update(email, updated);
            return RedirectToAction("Dashboard");
        }

        public IActionResult DeleteStudent(string email)
        {
            if (!IsAuthorized("teacher")) return RedirectToAction("Login", "User");

            _studentRepo.Delete(email);
            return RedirectToAction("Dashboard");
        }

        public IActionResult Profile()
        {
            if (!IsAuthorized("teacher")) return RedirectToAction("Login", "User");

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
