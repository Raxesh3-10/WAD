using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;

namespace SAS.Controllers
{
    public class UserDetailsController : Controller
    {
        private readonly IUserDetailsRepository _repository;
        private readonly IMapper _mapper;

        public UserDetailsController(IUserDetailsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Details()
        {
            var userId = GetUserIdFromSession();
            if (userId == null)
            {
                ViewBag.StatusMsg = "No user logged in.";
                return View(null);
            }

            var details = _repository.GetByUserId(userId.Value);
            if (details == null)
            {
                ViewBag.StatusMsg = "No details found.";
                return View(null);
            }

            var vm = _mapper.Map<UserDetailsViewModel>(details) ?? new UserDetailsViewModel();
            ViewData["EditMode"] = false;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleEdit()
        {
            var userId = GetUserIdFromSession();
            if (userId == null) return RedirectToAction("Details");

            var details = _repository.GetByUserId(userId.Value);
            if (details == null) return RedirectToAction("Details");

            var vm = _mapper.Map<UserDetailsViewModel>(details) ?? new UserDetailsViewModel();
            ViewData["EditMode"] = true;
            return View("Details", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDetails(UserDetailsViewModel updatedDetails)
        {
            var userId = GetUserIdFromSession();
            if (userId == null) return RedirectToAction("Details");

            var existing = _repository.GetByUserId(userId.Value);
            if (existing == null)
            {
                ViewBag.StatusMsg = "User not found.";
                return RedirectToAction("Details");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.StatusMsg = "Please correct the highlighted errors.";
                updatedDetails.EditMode = true;
                return View("Details", updatedDetails);
            }

            // Map VM -> Entity (simple string fields). Repository handles files/uploads.
            existing.Subjects = updatedDetails.Subjects ?? string.Empty;
            existing.Stds = updatedDetails.Stds ?? string.Empty;
            existing.Qualifications = updatedDetails.Qualifications ?? string.Empty;
            existing.Salary = updatedDetails.Salary;
            existing.Dob = updatedDetails.Dob;
            existing.Experience = updatedDetails.Experience;
            existing.JoiningDate = updatedDetails.JoiningDate;
            existing.Address = updatedDetails.Address ?? string.Empty;
            existing.Phone = updatedDetails.Phone ?? string.Empty;

            // Call repository to handle DB update + Cloudinary (photo/doc uploads & removals)
            var success = _repository.UpdateDetails(
                userId.Value,
                existing,
                updatedDetails.PhotoFile,
                updatedDetails.NewDocuments?.ToList(),
                updatedDetails.RemoveDocIndexes
            );

            ViewBag.StatusMsg = success ? "Details updated successfully" : "Failed to update user details";

            // Re-fetch fresh entity (so vm contains uploaded photo/doc URLs)
            var refreshed = _repository.GetByUserId(userId.Value) ?? existing;
            var vm = _mapper.Map<UserDetailsViewModel>(refreshed);
            ViewData["EditMode"] = false;
            return View("Login", "User");
        }

        private Guid? GetUserIdFromSession()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return null;
            if (Guid.TryParse(userIdString, out var userId)) return userId;
            return null;
        }
    }
}
