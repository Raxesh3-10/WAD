using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.Controllers
{
    public class PrincipalController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IUserDetailsRepository _userDetailsRepo;
        private readonly IMapper _mapper;

        public PrincipalController(
            IRepository<User> userRepo,
            IUserDetailsRepository userDetailsRepo,
            IMapper mapper
        )
        {
            _userRepo = userRepo;
            _userDetailsRepo = userDetailsRepo;
            _mapper = mapper;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized("principal")) return HandleUnauthorized();

            var currentUser = GetCurrentUser();
            ViewBag.Profile = _mapper.Map<UserViewModel>(currentUser);

            var teachers = _userRepo.GetAll().Where(u => u.Role == UserRole.Teacher).ToList();
            var staffs = _userRepo.GetAll().Where(u => u.Role == UserRole.Staff).ToList();

            ViewBag.Teachers = teachers.Select(t => BuildUserDetailsVM(t)).ToList();
            ViewBag.Staffs = staffs.Select(s => BuildUserDetailsVM(s)).ToList();

            return View();
        }

        private bool IsAuthorized(string role) =>
            HttpContext.Session.GetString("UserRole") == role;

        private User? GetCurrentUser()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            return _userRepo.GetByEmail(email ?? "");
        }
        private IActionResult HandleUnauthorized()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
        private DetailsViewModel BuildUserDetailsVM(User user)
        {
            var userDetails = _userDetailsRepo.GetByUserId(user.Id);
            var userVM = _mapper.Map<UserViewModel>(user);
            var detailsVM = userDetails != null ? _mapper.Map<UserDetailsViewModel>(userDetails) : null;

            return new DetailsViewModel
            {
                user = userVM,
                details = detailsVM
            };
        }
    }
}
