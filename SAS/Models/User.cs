using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public enum UserRole
    {
        Teacher,
        Principal,
        Trustee
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "User role is required.")]
        public UserRole Role { get; set; }

        public ICollection<Notice> Notices { get; set; } = new List<Notice>();

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