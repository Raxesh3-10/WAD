using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Models.Repositories;
using System.Linq;

namespace SAS.Controllers
{
    public class PrincipalController : Controller
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<User> _userRepo;

        public PrincipalController(IRepository<Student> studentRepo, IRepository<User> userRepo)
        {
            _studentRepo = studentRepo;
            _userRepo = userRepo;
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

            ViewBag.Profile = GetCurrentUser();
            ViewBag.Teachers = teachers;
            ViewBag.GroupedStudents = grouped;

            return View(students);
        }


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
