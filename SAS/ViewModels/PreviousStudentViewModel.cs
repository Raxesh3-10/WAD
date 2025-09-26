using System;
using System.ComponentModel.DataAnnotations;

namespace SAS.ViewModels
{
    public class PreviousStudentViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string StudentName { get; set; }

        public string? PhotoUrl { get; set; }

        public string FatherName { get; set; }

        public string MotherName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Range(100000000000, 999999999999, ErrorMessage = "Aadhar number must be exactly 12 digits.")]
        public long AadharNo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Roll number must be a positive number.")]
        public int RollNo { get; set; }

        [RegularExpression(@"^[A-Z]$", ErrorMessage = "Division must be a single uppercase letter (e.g. A, B, C).")]
        public string Div { get; set; }

        [Range(1, 12, ErrorMessage = "Standard must be between 1 and 12.")]
        public int Std { get; set; }

        [Range(1000000000, 9999999999, ErrorMessage = "Phone number must be 10 digits and cannot start with 0.")]
        public long PhoneNo { get; set; }

        [Required(ErrorMessage = "Passing year is required.")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Year must be in format YYYY-YYYY (e.g. 2021-2022).")]
        public string PassingYear { get; set; }
    }
}