using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System.Linq;

namespace SAS.Controllers
{
    public class StaffController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;

        public StaffController(
            IRepository<User> userRepo,
            IMapper mapper
        )
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized("staff")) return Unauthorized();

            var currentUser = GetCurrentUser();
            ViewBag.Profile = _mapper.Map<UserViewModel>(currentUser);

            return View();
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