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
            return _context.Users.ToList();
        }

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public void Add(User user)
        {
            user.Password = user.GetHashedPassword(); // Hash password
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public bool Update(string email, User updatedUser)
        {
            var existing = _context.Users.FirstOrDefault(u => u.Email == email);
            if (existing == null) return false;

            // Hash password if it's not already hashed
            if (!BCrypt.Net.BCrypt.Verify("test", updatedUser.Password))
            {
                updatedUser.Password = updatedUser.GetHashedPassword();
            }

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
