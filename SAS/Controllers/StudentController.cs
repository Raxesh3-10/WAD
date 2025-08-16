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

        public IActionResult CreateStudent([FromBody] StudentViewModel studentVm)
        {
            if (!IsAuthorized("principal")) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var student = _mapper.Map<Student>(studentVm);
            _studentRepo.Add(student);

            var resultVm = _mapper.Map<StudentViewModel>(student);
            return Ok(new { message = "Student created successfully", student = resultVm });
        }

        public IActionResult EditStudent(string email, [FromBody] StudentViewModel studentVm)
        {
            if (!IsAuthorized("principal")) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedStudent = _mapper.Map<Student>(studentVm);
            _studentRepo.Update(email, updatedStudent);

            var resultVm = _mapper.Map<StudentViewModel>(updatedStudent);
            return Ok(new { message = "Student updated successfully", student = resultVm });
        }

        public IActionResult DeleteStudent(string email)
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var student = _studentRepo.GetByEmail(email);
            if (student == null) return NotFound(new { message = "Student not found" });

            _studentRepo.Delete(email);
            return Ok(new { message = "Student deleted successfully" });
        }

        public IActionResult GetStudent(string email)
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var student = _studentRepo.GetByEmail(email);
            if (student == null) return NotFound(new { message = "Student not found" });

            var studentVm = _mapper.Map<StudentViewModel>(student);
            return Ok(studentVm);
        }

        public IActionResult GetAllStudents()
        {
            if (!IsAuthorized("principal")) return Unauthorized();

            var students = _studentRepo.GetAll().ToList();
            var studentVms = students.Select(s => _mapper.Map<StudentViewModel>(s)).ToList();
            return Ok(studentVms);
        }

        private bool IsAuthorized(string role) =>
            HttpContext.Session.GetString("UserRole") == role;
    }
}