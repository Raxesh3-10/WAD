using System;
using System.Collections.Generic;
using System.Linq;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        private string? UploadToCloudinary(IFormFile file, string folder)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult?.SecureUrl?.ToString();
        }

        private void DeleteFromCloudinary(string? url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var parts = url.Split('/');
                var filename = parts.Last().Split('.')[0];
                var uploadIndex = Array.IndexOf(parts, "upload");
                string folder = uploadIndex >= 0
                    ? string.Join("/", parts.Skip(uploadIndex + 1).Take(parts.Length))
                    : string.Empty;

                var publicId = string.IsNullOrEmpty(folder) ? filename : $"{folder}/{filename}";
                _cloudinary.Destroy(new DeletionParams(publicId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting from Cloudinary: {ex.Message}");
            }
        }

        public IEnumerable<UserDetails> GetAll()
        {
            return _context.UserDetails
                .Include(u => u.Subjects)
                .Include(u => u.Stds)
                .Include(u => u.Qualifications)
                .Include(u => u.Documents)
                .ToList();
        }

        public UserDetails? GetByUserId(Guid userId)
        {
            return _context.UserDetails
                .Include(u => u.Subjects)
                .Include(u => u.Stds)
                .Include(u => u.Qualifications)
                .Include(u => u.Documents)
                .FirstOrDefault(d => d.UserId == userId);
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
            };

            _context.UserDetails.Add(emptyDetails);
            _context.SaveChanges();
            return emptyDetails;
        }

        public bool UpdateDetails(Guid userId, UserDetails updatedDetails, IFormFile? photo, List<IFormFile>? documents)
        {
            var existing = GetByUserId(userId);
            if (existing == null) return false;

            if (photo != null)
            {
                if (!string.IsNullOrEmpty(existing.Photo))
                    DeleteFromCloudinary(existing.Photo);

                existing.Photo = UploadToCloudinary(photo, "photos");
            }

            if (documents != null && documents.Count > 0)
            {
                foreach (var doc in existing.Documents.ToList())
                    DeleteFromCloudinary(doc.DocumentUrl);

                existing.Documents.Clear();
                foreach (var doc in documents)
                {
                    var url = UploadToCloudinary(doc, "documents");
                    if (url != null)
                    {
                        existing.Documents.Add(new UserDocument
                        {
                            DocumentUrl = url,
                            UserDetailsId = existing.Id
                        });
                    }
                }
            }

            existing.Subjects.Clear();
            foreach (var subj in updatedDetails.Subjects.Select(s => s.SubjectName))
            {
                existing.Subjects.Add(new UserSubject { SubjectName = subj, UserDetailsId = existing.Id });
            }

            existing.Stds.Clear();
            foreach (var std in updatedDetails.Stds.Select(s => s.Std))
            {
                existing.Stds.Add(new UserStd { Std = std, UserDetailsId = existing.Id });
            }

            existing.Qualifications.Clear();
            foreach (var qual in updatedDetails.Qualifications.Select(q => q.QualificationName))
            {
                existing.Qualifications.Add(new UserQualification { QualificationName = qual, UserDetailsId = existing.Id });
            }

            existing.Salary = updatedDetails.Salary;
            existing.Experience = updatedDetails.Experience;
            existing.Dob = updatedDetails.Dob;
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
                DeleteFromCloudinary(existing.Photo);

            foreach (var doc in existing.Documents.ToList())
                DeleteFromCloudinary(doc.DocumentUrl);

            _context.UserDetails.Remove(existing);
            _context.SaveChanges();
            return true;
        }
    }
}