using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using SAS.Models;

namespace SAS.Repositories
{
    public class SQLUserDetailsRepository : IUserDetailsRepository
    {
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;

        public SQLUserDetailsRepository(AppDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        #region Cloudinary Helpers
        private string UploadImage(IFormFile file, string folder)
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

        private string UploadFile(IFormFile file, string folder)
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

        public IEnumerable<UserDetails> GetAll()
        {
            return _context.UserDetails.ToList();
        }

        public UserDetails? GetByUserId(Guid userId)
        {
            return _context.UserDetails.FirstOrDefault(d => d.UserId == userId);
        }

        public UserDetails CreateEmptyDetails(Guid userId)
        {
            var emptyDetails = new UserDetails
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Salary = 0,
                Dob = DateTime.MinValue,
                Experience = 0,
                JoiningDate = DateTime.MinValue,
                Address = string.Empty,
                Phone = string.Empty,
                Subjects = string.Empty,
                Stds = string.Empty,
                Qualifications = string.Empty,
                Documents = string.Empty,
                Photo = string.Empty
            };

            _context.UserDetails.Add(emptyDetails);
            _context.SaveChanges();
            return emptyDetails;
        }

        public bool UpdateDetails(Guid userId, UserDetails updatedDetails, IFormFile? photo, List<IFormFile>? documents, List<int>? removeDocIndexes)
        {
            var existing = GetByUserId(userId);
            if (existing == null) return false;

            // --- Handle Photo ---
            if (photo != null)
            {
                if (!string.IsNullOrEmpty(existing.Photo))
                    DeleteFromCloudinary(existing.Photo, true);

                existing.Photo = UploadImage(photo, "photos");
            }

            // --- Handle Documents ---
            var docsList = string.IsNullOrWhiteSpace(existing.Documents)
                ? new List<string>()
                : existing.Documents.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToList();

            if (removeDocIndexes != null)
            {
                foreach (var index in removeDocIndexes.OrderByDescending(i => i))
                {
                    if (index >= 0 && index < docsList.Count)
                    {
                        DeleteFromCloudinary(docsList[index], false);
                        docsList.RemoveAt(index);
                    }
                }
            }

            if (documents != null && documents.Any())
            {
                foreach (var doc in documents)
                {
                    var url = UploadFile(doc, "documents");
                    if (!string.IsNullOrEmpty(url))
                        docsList.Add(url);
                }
            }

            existing.Documents = string.Join(", ", docsList);

            // --- Update simple fields ---
            existing.Subjects = updatedDetails.Subjects;
            existing.Stds = updatedDetails.Stds;
            existing.Qualifications = updatedDetails.Qualifications;
            existing.Salary = updatedDetails.Salary;
            existing.Dob = updatedDetails.Dob;
            existing.Experience = updatedDetails.Experience;
            existing.JoiningDate = updatedDetails.JoiningDate;
            existing.Address = updatedDetails.Address;
            existing.Phone = updatedDetails.Phone;

            _context.UserDetails.Update(existing);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteDetails(Guid userId)
        {
            var existing = GetByUserId(userId);
            if (existing == null) return false;

            if (!string.IsNullOrEmpty(existing.Photo))
                DeleteFromCloudinary(existing.Photo, true);

            if (!string.IsNullOrEmpty(existing.Documents))
            {
                foreach (var doc in existing.Documents.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    DeleteFromCloudinary(doc.Trim(), false);
            }

            _context.UserDetails.Remove(existing);
            _context.SaveChanges();
            return true;
        }
    }
}
