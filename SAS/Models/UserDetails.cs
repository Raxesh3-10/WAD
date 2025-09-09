using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAS.Models
{
    public class UserDetails
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public User User { get; set; }

        public string Subjects { get; set; }
        public string Stds { get; set; }
        public string Qualifications { get; set; }
        public string Documents { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        public decimal Salary { get; set; }

        [DataType(DataType.Date, ErrorMessage = "DOB must be a valid date.")]
        public DateTime Dob { get; set; }

        public string Photo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Experience must be a non-negative integer.")]
        public int Experience { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Joining Date must be a valid date.")]
        public DateTime JoiningDate { get; set; }

        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }
    }
}