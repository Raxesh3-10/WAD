using System;
using System.Linq;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace SAS.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public string UploadImage(IFormFile file, string folder)
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

        public string UploadFile(IFormFile file, string folder)
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

        public void DeleteFromCloudinary(string url, bool isImage)
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
    }
}