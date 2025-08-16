using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Notice> _noticeRepo;
        private readonly IMapper _mapper;

        public TeacherController(
            IRepository<Student> studentRepo,
            IRepository<User> userRepo,
            IRepository<Notice> noticeRepo,
            IMapper mapper
        )
        {
            _studentRepo = studentRepo;
            _userRepo = userRepo;
            _noticeRepo = noticeRepo;
            _mapper = mapper;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized("teacher")) return Unauthorized();

            var students = _studentRepo.GetAll();
            var grouped = students
                .GroupBy(s => s.Std)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.GroupBy(s => s.Div.ToUpper())
                          .ToDictionary(d => d.Key, d => _mapper.Map<StudentViewModel[]>(d.ToList()))
                );

            var notices = _noticeRepo.GetAll()
                .OrderByDescending(n => n.Date)
                .Select(n => _mapper.Map<NoticeViewModel>(n))
                .ToList();

            var profile = _mapper.Map<UserViewModel>(GetCurrentUser());

            return Ok(new
            {
                Profile = profile,
                GroupedStudents = grouped,
                Notices = notices
            });
        }

        public IActionResult Profile()
        {
            if (!IsAuthorized("teacher")) return Unauthorized();

            var profile = _mapper.Map<UserViewModel>(GetCurrentUser());
            return Ok(profile);
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