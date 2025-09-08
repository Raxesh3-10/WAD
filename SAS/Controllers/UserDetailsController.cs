using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SAS.Controllers
{
    public class UserDetailsController : Controller
    {
        private readonly IUserDetailsRepository _repository;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public UserDetailsController(IUserDetailsRepository repository, IMapper mapper, Cloudinary cloudinary)
        {
            _repository = repository;
            _mapper = mapper;
            _cloudinary = cloudinary;
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

            var vm = MapToViewModel(details);
            ViewData["EditMode"] = false;
            return View(vm);
        }

        [HttpPost]
        public IActionResult ToggleEdit()
        {
            var userId = GetUserIdFromSession();
            if (userId == null) return RedirectToAction("Details");

            var details = _repository.GetByUserId(userId.Value);
            if (details == null) return RedirectToAction("Details");

            var vm = MapToViewModel(details);
            ViewData["EditMode"] = true;
            return View("Details", vm);
        }

        [HttpPost]
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
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Validation Error: " + error.ErrorMessage);
                }

                ViewBag.StatusMsg = "Please correct the highlighted errors.";
                updatedDetails.EditMode = true;
                return View("Details", updatedDetails);
            }

            // Map updated values into entity
            MapViewModelToEntity(updatedDetails, existing);

            var success = _repository.UpdateDetails(userId.Value, existing, updatedDetails.PhotoFile, updatedDetails.NewDocuments?.ToList());

            ViewBag.StatusMsg = success ? "Details updated successfully" : "Failed to update user details";

            var vm = MapToViewModel(existing);
            ViewData["EditMode"] = false;
            return View("Details", vm);
        }

        #region Mapping Helpers
        private void MapViewModelToEntity(UserDetailsViewModel vm, UserDetails entity)
        {
            // Subjects
            entity.Subjects = vm.SubjectsText?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new UserSubject { SubjectName = s.Trim(), UserDetailsId = entity.Id })
                .ToList() ?? new List<UserSubject>();

            // Standards (safe parsing)
            entity.Stds = vm.StdText?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var std)
                    ? new UserStd { Std = std, UserDetailsId = entity.Id }
                    : null)
                .Where(s => s != null)
                .ToList() ?? new List<UserStd>();

            // Qualifications
            entity.Qualifications = vm.QualificationsText?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(q => new UserQualification { QualificationName = q.Trim(), UserDetailsId = entity.Id })
                .ToList() ?? new List<UserQualification>();

            // Basic fields
            entity.Salary = vm.Salary ?? 0;
            entity.Dob = vm.Dob;
            entity.Experience = vm.Experience ?? 0;
            entity.JoiningDate = vm.JoiningDate;
            entity.Address = vm.Address ?? string.Empty;
            entity.Phone = vm.Phone ?? string.Empty;

            // Photo
            if (vm.PhotoFile != null)
            {
                if (!string.IsNullOrEmpty(entity.Photo))
                    DeleteFromCloudinary(entity.Photo, isImage: true);

                entity.Photo = UploadImageToCloudinary(vm.PhotoFile, "photos");
            }

            // Documents
            var docsToKeep = entity.Documents?.ToList() ?? new List<UserDocument>();

            if (vm.RemoveDocIndexes != null)
            {
                foreach (var index in vm.RemoveDocIndexes)
                {
                    if (index >= 0 && index < docsToKeep.Count)
                    {
                        DeleteFromCloudinary(docsToKeep[index].DocumentUrl, isImage: false);
                        docsToKeep.RemoveAt(index);
                    }
                }
            }

            if (vm.NewDocuments != null && vm.NewDocuments.Any())
            {
                foreach (var doc in vm.NewDocuments)
                {
                    var docUrl = UploadFileToCloudinary(doc, "documents");
                    docsToKeep.Add(new UserDocument { DocumentUrl = docUrl, UserDetailsId = entity.Id });
                }
            }

            entity.Documents = docsToKeep;
        }

        private UserDetailsViewModel MapToViewModel(UserDetails details)
        {
            var vm = _mapper.Map<UserDetailsViewModel>(details);
            vm.UserEmail = details.User?.Email ?? string.Empty;
            vm.Dob = details.Dob;
            vm.JoiningDate = details.JoiningDate;

            vm.SubjectsText = details.Subjects?.Any() == true
                ? string.Join(", ", details.Subjects.Select(s => s.SubjectName))
                : string.Empty;

            vm.StdText = details.Stds?.Any() == true
                ? string.Join(", ", details.Stds.Select(s => s.Std))
                : string.Empty;

            vm.QualificationsText = details.Qualifications?.Any() == true
                ? string.Join(", ", details.Qualifications.Select(q => q.QualificationName))
                : string.Empty;

            vm.Documents = details.Documents?.Select(d => d.DocumentUrl).ToList();
            return vm;
        }
        #endregion

        #region Cloudinary Helpers
        private string UploadImageToCloudinary(IFormFile file, string folder)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
            var result = _cloudinary.Upload(uploadParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }

        private string UploadFileToCloudinary(IFormFile file, string folder)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
            var result = _cloudinary.Upload(uploadParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }

        private void DeleteFromCloudinary(string url, bool isImage)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var parts = url.Split('/');
                var filename = parts[^1].Split('.')[0];
                var folderIndex = Array.IndexOf(parts, "upload") + 1;
                var folder = folderIndex > 0 ? string.Join("/", parts[folderIndex..^1]) : string.Empty;

                var publicId = string.IsNullOrEmpty(folder) ? filename : $"{folder}/{filename}";
                _cloudinary.Destroy(new DeletionParams(publicId)
                {
                    ResourceType = isImage ? ResourceType.Image : ResourceType.Raw
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting from Cloudinary: " + ex.Message);
            }
        }
        #endregion

        private Guid? GetUserIdFromSession()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return null;
            if (Guid.TryParse(userIdString, out var userId)) return userId;
            return null;
        }
    }
}
