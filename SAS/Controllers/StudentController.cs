using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;
using System;

namespace SAS.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IPreviousStudentRepository _previousStudentRepo;
        private readonly IMapper _mapper;

        public StudentController(IStudentRepository studentRepo, IPreviousStudentRepository previousStudentRepo,IMapper mapper)
        {
            _studentRepo = studentRepo;
            _mapper = mapper;
            _previousStudentRepo = previousStudentRepo;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStudent(StudentViewModel studentVm)
        {
            if (!IsAuthorized("principal", "teacher")) return HandleUnauthorized();
            if (!ModelState.IsValid) return View("CreateStudent", studentVm);

            var student = _mapper.Map<Student>(studentVm);
            _studentRepo.Add(student, studentVm.PhotoFile);

            TempData["SuccessMessage"] = "Student created successfully.";
            return RedirectToRoleDashboard();
        }

        [HttpPost]
        public IActionResult EditStudent(StudentViewModel studentVm)
        {
            if (!IsAuthorized("principal", "teacher")) return HandleUnauthorized();
            if (!ModelState.IsValid) return View(studentVm);

            var updatedStudent = _mapper.Map<Student>(studentVm);
            _studentRepo.Update(studentVm.Email, updatedStudent, studentVm.PhotoFile);

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PromoteStudents()
        {
            if (!IsAuthorized("principal")) return HandleUnauthorized();

            var currentYear = DateTime.Now.Month >= 6
                ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";

            var students = _studentRepo.GetAll().ToList();

            foreach (var student in students)
            {
                var prevStudent = new PreviousStudent
                {
                    Id = Guid.NewGuid(),
                    StudentName = student.StudentName,
                    PhotoUrl = student.PhotoUrl,
                    FatherName = student.FatherName,
                    MotherName = student.MotherName,
                    Email = student.Email,
                    AadharNo = student.AadharNo,
                    RollNo = student.RollNo,
                    Div = student.Div,
                    Std = student.Std,
                    PhoneNo = student.PhoneNo,
                    PassingYear = currentYear
                };
                _previousStudentRepo.Add(prevStudent);
                student.Std++;

                if (student.Std > 12)
                {
                    _studentRepo.Delete(student.Email);
                }
                else
                {
                    _studentRepo.Update(student.Email, student, null);
                }
            }
            return RedirectToRoleDashboard();
        }
        private bool IsAuthorized(params string[] allowedRoles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role != null && allowedRoles.Contains(role);
        }

        private IActionResult HandleUnauthorized()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
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