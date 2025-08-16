using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public class UserDetails
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public User User { get; set; }

        public ICollection<string> Subjects { get; set; } = new List<string>();

        public ICollection<int> Std { get; set; } = new List<int>();

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date, ErrorMessage = "DOB must be a valid date.")]
        public DateTime Dob { get; set; }

        [Url(ErrorMessage = "Photo must be a valid URL.")]
        public string? Photo { get; set; }

        public ICollection<string> Qualifications { get; set; } = new List<string>();

        [Range(0, int.MaxValue, ErrorMessage = "Experience must be a non-negative integer.")]
        public int Experience { get; set; }

        [Required(ErrorMessage = "Joining Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Joining Date must be a valid date.")]
        public DateTime JoiningDate { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }

        public ICollection<string> Documents { get; set; } = new List<string>();
    }
}