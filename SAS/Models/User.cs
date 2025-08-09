using System;
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
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "User role is required.")]
        public UserRole Role { get; set; }

        [Display(Name = "Subject")]
        public string? Subject { get; set; }

        [Range(1, 12, ErrorMessage = "Standard must be between 1 and 12.")]
        public int? Std { get; set; }

        [RegularExpression(@"^[A-Z]$", ErrorMessage = "Division must be a single uppercase letter (e.g. A, B, C).")]
        public string? Div { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        public decimal? Salary { get; set; }

        public string GetHashedPassword() =>
            BCrypt.Net.BCrypt.HashPassword(Password);

        public bool VerifyPassword(string inputPassword) =>
            BCrypt.Net.BCrypt.Verify(inputPassword, Password);

        public bool HasRole(UserRole role) =>
            Role == role;

        public object GetProfile()
        {
            if (Role == UserRole.Teacher || Role == UserRole.Principal)
            {
                return new
                {
                    Name,
                    Email,
                    Role,
                    Subject,
                    Std,
                    Div,
                    Salary
                };
            }

            return new
            {
                Name,
                Email,
                Role
            };
        }
    }
}
