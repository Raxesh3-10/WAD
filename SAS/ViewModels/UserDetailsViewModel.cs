using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SAS.ViewModels
{
    public class UserDetailsViewModel
    {
        [Required(ErrorMessage = "Subjects are required.")]
        public string Subjects { get; set; } = string.Empty;

        [Required(ErrorMessage = "Standards are required.")]
        public string Stds { get; set; } = string.Empty;

        [Required(ErrorMessage = "Qualifications are required.")]
        public string Qualifications { get; set; } = string.Empty;

        public string Documents { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        [Required(ErrorMessage = "Salary is required.")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Experience must be non-negative.")]
        [Required(ErrorMessage = "Experience is required.")]
        public int Experience { get; set; }

        [Required(ErrorMessage = "Joining date is required.")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        [Required(ErrorMessage = "Phone number is required.")]
        public string Phone { get; set; }

        public string? Photo { get; set; }

        public IFormFile? PhotoFile { get; set; }
        public List<IFormFile>? NewDocuments { get; set; }
        public List<int>? RemoveDocIndexes { get; set; } = new List<int>();

        // UI control flags
        public bool EditMode { get; set; } = false;
        public bool IsStaff { get; set; } = false;
    }
}