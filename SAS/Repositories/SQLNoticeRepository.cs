using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SAS.Models;
using SAS.Services;

namespace SAS.Repositories
{
    public class SQLNoticeRepository : INoticeRepository
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public SQLNoticeRepository(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public IEnumerable<Notice> GetAll()
        {
            return _context.Notices
                .Include(n => n.User)
                .ToList();
        }

        public Notice? GetByEmail(string email)
        {
            return _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.User.Email == email);
        }

        public void Add(Notice notice, List<IFormFile>? Documentss = null)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == notice.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (notice.NoticeId == Guid.Empty)
                notice.NoticeId = Guid.NewGuid();

            if (Documentss != null && Documentss.Count > 0)
            {
                var urls = new List<string>();
                foreach (var file in Documentss)
                {
                    var url = _cloudinaryService.UploadFile(file, "documents");
                    if (!string.IsNullOrEmpty(url))
                        urls.Add(url);
                }

                notice.Documents = string.Join(",", urls);
            }

            _context.Notices.Add(notice);
            _context.SaveChanges();
        }

        public bool Update(string email, Notice updatedNotice, List<IFormFile>? newDocumentss = null)
        {
            var existing = _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.NoticeId == updatedNotice.NoticeId &&
                                     n.User.Email == email);

            if (existing == null) return false;

            existing.Subject = updatedNotice.Subject;
            existing.Message = updatedNotice.Message;
            existing.Date = updatedNotice.Date;

            if (newDocumentss != null && newDocumentss.Count > 0)
            {
                var urls = new List<string>();
                foreach (var file in newDocumentss)
                {
                    var url = _cloudinaryService.UploadFile(file, "documents");
                    if (!string.IsNullOrEmpty(url))
                        urls.Add(url);
                }

                if (!string.IsNullOrEmpty(existing.Documents))
                    existing.Documents += "," + string.Join(",", urls);
                else
                    existing.Documents = string.Join(",", urls);
            }

            _context.SaveChanges();
            return true;
        }

        public bool Delete(string email)
        {
            var notice = _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.User.Email == email);

            if (notice == null) return false;

            DeleteDocumentssFromCloudinary(notice.Documents);

            _context.Notices.Remove(notice);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteById(Guid id)
        {
            var notice = _context.Notices.FirstOrDefault(n => n.NoticeId == id);
            if (notice == null) return false;

            DeleteDocumentssFromCloudinary(notice.Documents);

            _context.Notices.Remove(notice);
            _context.SaveChanges();
            return true;
        }

        private void DeleteDocumentssFromCloudinary(string? DocumentsUrls)
        {
            if (string.IsNullOrEmpty(DocumentsUrls)) return;

            var urls = DocumentsUrls.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var url in urls)
            {
                _cloudinaryService.DeleteFromCloudinary(url, isImage: false);
            }
        }
    }
}