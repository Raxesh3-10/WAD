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
            if (!IsAuthorized("principal")) return Unauthorized();
            if (!ModelState.IsValid) return ViewComponent("Student");

            var student = _mapper.Map<Student>(studentVm);
            _studentRepo.Add(student);

            TempData["SuccessMessage"] = "Student created successfully.";
            return RedirectToAction("Dashboard", "Principal"); 
        }

        [HttpGet]
        public IActionResult EditStudent(string email)
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var student = _studentRepo.GetByEmail(email);
            if (student == null) return NotFound();

            var vm = _mapper.Map<StudentViewModel>(student);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStudent(StudentViewModel studentVm)
        {
            if (!IsAuthorized("principal")) return Unauthorized();
            if (!ModelState.IsValid) return View(studentVm);

            var updatedStudent = _mapper.Map<Student>(studentVm);
            _studentRepo.Update(studentVm.Email, updatedStudent);

            TempData["SuccessMessage"] = "Student updated successfully.";
            return RedirectToAction("Dashboard", "Principal");
        }

        [HttpGet]
        public IActionResult DeleteStudent(string email)
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var student = _studentRepo.GetByEmail(email);
            if (student == null) return NotFound();

            _studentRepo.Delete(email);

            TempData["SuccessMessage"] = "Student deleted successfully.";
            return RedirectToAction("Dashboard", "Principal");
        }

        [HttpGet]
        public IActionResult GetStudent(string email)
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var student = _studentRepo.GetByEmail(email);
            if (student == null) return NotFound();

            var studentVm = _mapper.Map<StudentViewModel>(student);
            return View(studentVm);
        }

        [HttpGet]
        public IActionResult GetAllStudents()
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var students = _studentRepo.GetAll().ToList();
            var studentVms = students.Select(s => _mapper.Map<StudentViewModel>(s)).ToList();

            return View(studentVms);
        }

        private bool IsAuthorized(string role) =>
            HttpContext.Session.GetString("UserRole") == role;
    }
}