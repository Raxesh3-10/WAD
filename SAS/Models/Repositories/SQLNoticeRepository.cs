using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SAS.Models;

namespace SAS.Models.Repositories
{
    public class SQLNoticeRepository : IRepository<Notice>
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

        public Notice GetByEmail(string email)
        {
            return _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.User.Email == email);
        }

        public void Add(Notice notice)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == notice.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            _context.Notices.Add(notice);
            _context.SaveChanges();
        }

        public bool Update(string email, Notice updatedNotice)
        {
            var existing = _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.NoticeId == updatedNotice.NoticeId &&
                                     n.User.Email == email);

            if (existing == null) return false;

            existing.Subject = updatedNotice.Subject;
            existing.Message = updatedNotice.Message;
            existing.Date = updatedNotice.Date;

            _context.SaveChanges();
            return true;
        }

        public bool Delete(string email)
        {
            var notice = _context.Notices
                .Include(n => n.User)
                .FirstOrDefault(n => n.User.Email == email);

            if (notice == null) return false;

            _context.Notices.Remove(notice);
            _context.SaveChanges();
            return true;
        }

        // Extra: DELETE by NoticeId
        public bool DeleteById(int id)
        {
            var notice = _context.Notices.Find(id);
            if (notice == null) return false;

            _context.Notices.Remove(notice);
            _context.SaveChanges();
            return true;
        }
    }
}
