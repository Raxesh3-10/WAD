using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SAS.Models;
using SAS.Services;

namespace SAS.Repositories
{
    public class SQLUserDetailsRepository : IUserDetailsRepository
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public SQLUserDetailsRepository(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

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

            if (photo != null)
            {
                if (!string.IsNullOrEmpty(existing.Photo))
                    _cloudinaryService.DeleteFromCloudinary(existing.Photo, true);

                existing.Photo = _cloudinaryService.UploadImage(photo, "photos");
            }

            var docsList = string.IsNullOrWhiteSpace(existing.Documents)
                ? new List<string>()
                : existing.Documents.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToList();

            if (removeDocIndexes != null)
            {
                foreach (var index in removeDocIndexes.OrderByDescending(i => i))
                {
                    if (index >= 0 && index < docsList.Count)
                    {
                        _cloudinaryService.DeleteFromCloudinary(docsList[index], false);
                        docsList.RemoveAt(index);
                    }
                }
            }

            if (documents != null && documents.Any())
            {
                foreach (var doc in documents)
                {
                    var url = _cloudinaryService.UploadFile(doc, "documents");
                    if (!string.IsNullOrEmpty(url))
                        docsList.Add(url);
                }
            }

            existing.Documents = string.Join(", ", docsList);

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
                _cloudinaryService.DeleteFromCloudinary(existing.Photo, true);

            if (!string.IsNullOrEmpty(existing.Documents))
            {
                foreach (var doc in existing.Documents.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    _cloudinaryService.DeleteFromCloudinary(doc.Trim(), false);
            }

            _context.UserDetails.Remove(existing);
            _context.SaveChanges();
            return true;
        }
    }
}