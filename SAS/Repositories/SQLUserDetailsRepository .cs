using System.Collections.Generic;
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

        public UserDetails GetByUserId(int userId)
        {
            return _context.UserDetails
                .FirstOrDefault(d => d.User.Id == userId);
        }

        public bool UpdateDetails(int userId, UserDetails updatedDetails, IFormFile? photo, List<IFormFile>? documents)
        {
            var existing = GetByUserId(userId);
            if (existing == null) return false;

            if (photo != null)
            {
                using var stream = photo.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(photo.FileName, stream),
                    Folder = "photos"
                };
                var uploadResult = _cloudinary.Upload(uploadParams); // synchronous
                existing.Photo = uploadResult.SecureUrl.ToString();
            }

            if (documents != null && documents.Count > 0)
            {
                var urls = new List<string>();
                foreach (var doc in documents)
                {
                    using var stream = doc.OpenReadStream();
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(doc.FileName, stream),
                        Folder = "documents"
                    };
                    var uploadResult = _cloudinary.Upload(uploadParams); // synchronous
                    urls.Add(uploadResult.SecureUrl.ToString());
                }
                existing.Documents = urls;
            }

            existing.Subjects = updatedDetails.Subjects;
            existing.Std = updatedDetails.Std;
            existing.Salary = updatedDetails.Salary;
            existing.Experience = updatedDetails.Experience;
            existing.Dob = updatedDetails.Dob;
            existing.JoiningDate = updatedDetails.JoiningDate;
            existing.Address = updatedDetails.Address;
            existing.Phone = updatedDetails.Phone;
            existing.Qualifications = updatedDetails.Qualifications;

            _context.UserDetails.Update(existing);
            _context.SaveChanges();
            return true;
        }
    }
}