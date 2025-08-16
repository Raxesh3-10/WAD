using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAS.ViewModels
{
    public class UserDetailsViewModel
    {
        [Required(ErrorMessage = "User email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string UserEmail { get; set; } = string.Empty;

        public ICollection<string>? Subjects { get; set; }
        public ICollection<int>? Std { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        public string? Photo { get; set; }
        public ICollection<string>? Qualifications { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Experience must be non-negative.")]
        public int Experience { get; set; }

        [Required(ErrorMessage = "Joining Date is required.")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; }

        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }

        public ICollection<string>? Documents { get; set; }
    }
}