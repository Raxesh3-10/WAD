using System;
using System.Collections.Generic;
using System.Linq;
using SAS.Models;
using Microsoft.EntityFrameworkCore;

namespace SAS.Repositories
{
    public class SQLUserRepository : IRepository<User>
    {
        private readonly AppDbContext _context;

        public SQLUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users
                .Include(u => u.Notices)
                .Include(u => u.UserDetails)
                .ToList();
        }

        public User? GetByEmail(string email)
        {
            return _context.Users
                .Include(u => u.Notices)
                .Include(u => u.UserDetails)
                .FirstOrDefault(u => u.Email == email);
        }

        public void Add(User user)
        {
            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            user.Password = user.GetHashedPassword();
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public bool Update(string email, User updatedUser)
        {
            var existing = _context.Users
                .Include(u => u.Notices)
                .Include(u => u.UserDetails)
                .FirstOrDefault(u => u.Email == email);

            if (existing == null) return false;

            updatedUser.Id = existing.Id;

            if (!string.IsNullOrEmpty(updatedUser.Password) &&
                !BCrypt.Net.BCrypt.Verify(updatedUser.Password, existing.Password))
                updatedUser.Password = updatedUser.GetHashedPassword();
            else
                updatedUser.Password = existing.Password;

            _context.Entry(existing).CurrentValues.SetValues(updatedUser);
            _context.SaveChanges();
            return true;
        }

        public bool Delete(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return false;

            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }
    }
}