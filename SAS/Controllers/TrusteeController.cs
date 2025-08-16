using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.Controllers
{
    public class TrusteeController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Notice> _noticeRepo;
        private readonly IMapper _mapper;

        public TrusteeController(
            IRepository<User> userRepo,
            IRepository<Notice> noticeRepo,
            IMapper mapper
        )
        {
            _userRepo = userRepo;
            _noticeRepo = noticeRepo;
            _mapper = mapper;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized("trustee")) return Unauthorized();

            var allUsers = _userRepo.GetAll();
            var teachers = allUsers
                .Where(u => u.Role == UserRole.Teacher)
                .Select(u => _mapper.Map<UserViewModel>(u))
                .ToList();
            var principals = allUsers
                .Where(u => u.Role == UserRole.Principal)
                .Select(u => _mapper.Map<UserViewModel>(u))
                .ToList();

            var notices = _noticeRepo.GetAll()
                .OrderByDescending(n => n.Date)
                .Select(n => _mapper.Map<NoticeViewModel>(n))
                .ToList();

            var profile = _mapper.Map<UserViewModel>(GetCurrentUser());

            return Ok(new
            {
                Profile = profile,
                Teachers = teachers,
                Principals = principals,
                Notices = notices
            });
        }
        public IActionResult Profile()
        {
            if (!IsAuthorized("trustee")) return Unauthorized();

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