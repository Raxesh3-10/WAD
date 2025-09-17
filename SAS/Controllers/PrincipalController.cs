using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.Services;
using AutoMapper;
using System.Linq;
using System.IO;
using QuestPDF.Fluent;
using SAS.ViewModels;

namespace SAS.Controllers
{
    public class PrincipalController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IUserDetailsRepository _userDetailsRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IdCardService _idCardService;
        private readonly IMapper _mapper;

        public PrincipalController(
            IRepository<User> userRepo,
            IUserDetailsRepository userDetailsRepo,
            IStudentRepository studentRepo,
            IPreviousStudentRepository previousStudentRepo,
            IdCardService idCardService,
            IMapper mapper
        )
        {
            _userRepo = userRepo;
            _userDetailsRepo = userDetailsRepo;
            _studentRepo = studentRepo;
            _idCardService = idCardService;
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

        [HttpPost]
        public IActionResult GenerateAllStudentIdCards()
        {
            var students = _studentRepo.GetAll().ToList();
            if (!students.Any()) return NotFound("No students found.");

            var stream = new MemoryStream();
            Document.Create(container =>
            {
                foreach (var s in students)
                {
                    container.Page(page => _idCardService.BuildStudentIdCard(page, s));
                }
            }).GeneratePdf(stream);

            stream.Position = 0;
            return File(stream, "application/pdf", "AllStudents_IdCards.pdf");
        }

        [HttpPost]
        public IActionResult GenerateAllUserIdCards()
        {
            var users = _userRepo.GetAll()
                .Where(u => u.Role == UserRole.Teacher || u.Role == UserRole.Principal || u.Role == UserRole.Staff)
                .ToList();

            if (!users.Any()) return NotFound("No teachers, principal, or staff found.");

            var stream = new MemoryStream();
            Document.Create(container =>
            {
                foreach (var u in users)
                {
                    container.Page(page => _idCardService.BuildUserIdCard(page, u));
                }
            }).GeneratePdf(stream);

            stream.Position = 0;
            return File(stream, "application/pdf", "AllUsers_IdCards.pdf");
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