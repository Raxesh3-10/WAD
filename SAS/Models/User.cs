using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public enum UserRole
    {
        Teacher,
        Principal,
        Trustee,
        Staff
    }

    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }


        public UserRole Role { get; set; }

        public ICollection<Notice> Notices { get; set; } = new List<Notice>();
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();

        public UserDetails UserDetails { get; set; }

        public string GetHashedPassword() =>
            BCrypt.Net.BCrypt.HashPassword(Password);

        public bool VerifyPassword(string inputPassword) =>
            BCrypt.Net.BCrypt.Verify(inputPassword, Password);

        public bool HasRole(UserRole role) =>
            Role == role;

        public object GetProfile()
        {
            return new
            {
                Name,
                Email,
                Role
            };
        }
    }
}