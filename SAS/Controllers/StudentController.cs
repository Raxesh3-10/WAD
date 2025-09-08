using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.Controllers
{
    public class StudentController : Controller
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IMapper _mapper;

        public StudentController(IRepository<Student> studentRepo, IMapper mapper)
        {
            _studentRepo = studentRepo;
            _mapper = mapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStudent(StudentViewModel studentVm)
        {
            if (!IsAuthorized("principal", "teacher")) return HandleUnauthorized();
            if (!ModelState.IsValid) return View("CreateStudent", studentVm);

            var student = _mapper.Map<Student>(studentVm);
            _studentRepo.Add(student);

            TempData["SuccessMessage"] = "Student created successfully.";
            return RedirectToRoleDashboard();
        }

        [HttpPost]
        public IActionResult EditStudent(StudentViewModel studentVm)
        {
            if (!IsAuthorized("principal", "teacher")) return HandleUnauthorized();
            if (!ModelState.IsValid) return View(studentVm);

            var updatedStudent = _mapper.Map<Student>(studentVm);
            _studentRepo.Update(studentVm.Email, updatedStudent);

            TempData["SuccessMessage"] = "Student updated successfully.";
            return RedirectToRoleDashboard();
        }

        [HttpPost]
        public IActionResult DeleteStudentConfirmed(string email)
        {
            if (!IsAuthorized("principal", "teacher")) return HandleUnauthorized();

            var student = _studentRepo.GetByEmail(email);
            if (student == null) return NotFound();

            _studentRepo.Delete(email);

            TempData["SuccessMessage"] = "Student deleted successfully.";
            return RedirectToRoleDashboard();
        }

        [HttpGet]
        public IActionResult GetStudent(string email)
        {
            if (!IsAuthorized("principal", "teacher")) return HandleUnauthorized();
            var student = _studentRepo.GetByEmail(email);
            if (student == null) return NotFound();
            var studentVm = _mapper.Map<StudentViewModel>(student);
            return PartialView("_EditStudent", studentVm);
        }

        [HttpGet]
        public IActionResult GetAllStudents()
        {
            if (!IsAuthorized("principal", "teacher")) return HandleUnauthorized();

            var students = _studentRepo.GetAll().ToList();
            var studentVms = students.Select(s => _mapper.Map<StudentViewModel>(s)).ToList();

            return View(studentVms);
        }

        // ---------- Helpers ----------
        private bool IsAuthorized(params string[] allowedRoles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role != null && allowedRoles.Contains(role);
        }

        private IActionResult HandleUnauthorized()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User"); // ensure this matches your Login controller
        }

        private IActionResult RedirectToRoleDashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role switch
            {
                "principal" => RedirectToAction("Dashboard", "Principal"),
                "teacher" => RedirectToAction("Dashboard", "Teacher"),
                _ => HandleUnauthorized()
            };
        }
    }
}