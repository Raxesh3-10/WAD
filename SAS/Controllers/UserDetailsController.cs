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
                updatedDetails.EditMode = true;
                return View("Details", updatedDetails);
            }

            // Map updated values into existing entity
            MapViewModelToEntity(updatedDetails, existing);

            // Save through repository
            var success = _repository.UpdateDetails(userId.Value, existing, updatedDetails.PhotoFile, updatedDetails.NewDocuments?.ToList());

            ViewBag.StatusMsg = success ? "Details updated successfully" : "Failed to update user details";

            var vm = MapToViewModel(existing);
            ViewData["EditMode"] = false;
            return View("Details", vm);
        }

        #region Mapping Helpers
        private void MapViewModelToEntity(UserDetailsViewModel vm, UserDetails entity)
        {
            // Collections
            entity.Subjects = vm.SubjectsText?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new UserSubject { SubjectName = s.Trim(), UserDetailsId = entity.Id })
                .ToList() ?? new List<UserSubject>();

            entity.Stds = vm.StdText?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new UserStd { Std = int.Parse(s.Trim()), UserDetailsId = entity.Id })
                .ToList() ?? new List<UserStd>();

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
                    DeleteFromCloudinary(entity.Photo);

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
                        DeleteFromCloudinary(docsToKeep[index].DocumentUrl);
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

        private void DeleteFromCloudinary(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var parts = url.Split('/');
                var filename = parts[^1].Split('.')[0];
                var folderIndex = Array.IndexOf(parts, "upload") + 1;
                var folder = folderIndex > 0 ? string.Join("/", parts[folderIndex..^1]) : string.Empty;

                var publicId = string.IsNullOrEmpty(folder) ? filename : $"{folder}/{filename}";
                _cloudinary.Destroy(new DeletionParams(publicId));
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
            return Guid.Parse(userIdString);
        }
    }
}
