using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SAS.Models;

namespace SAS.Repositories
{
    public class SQLNoticeRepository : INoticeRepository
    {
        private readonly AppDbContext _context;

        public SQLNoticeRepository(AppDbContext context)
        {
            _context = context;
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

        public Notice? GetById(Guid id)
        {
            return _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.NoticeId == id);
        }

        public void Add(Notice notice)
        {
            if (notice.NoticeId == Guid.Empty)
                notice.NoticeId = Guid.NewGuid();

            _context.Notices.Add(notice);
            _context.SaveChanges();
        }

        public bool Update(Notice updatedNotice)
        {
            var existing = _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.NoticeId == updatedNotice.NoticeId);

            if (existing == null) return false;

            existing.Subject = updatedNotice.Subject;
            existing.Message = updatedNotice.Message;
            existing.Date = updatedNotice.Date;
            existing.Documents = updatedNotice.Documents;

            _context.SaveChanges();
            return true;
        }

        public bool DeleteById(Guid id)
        {
            var notice = _context.Notices.FirstOrDefault(n => n.NoticeId == id);
            if (notice == null) return false;

            _context.Notices.Remove(notice);
            _context.SaveChanges();
            return true;
        }
    }
}