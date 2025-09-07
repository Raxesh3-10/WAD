using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SAS.ViewModels
{
    public class UserDetailsViewModel
    {
        [Required(ErrorMessage = "User email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string UserEmail { get; set; } = string.Empty;

        public List<string>? Subjects { get; set; }
        public List<int>? Std { get; set; }
        public List<string>? Qualifications { get; set; }

        public string? SubjectsText { get; set; }
        public string? StdText { get; set; }
        public string? QualificationsText { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        public decimal? Salary { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Experience must be non-negative.")]
        public int? Experience { get; set; }

        [Required(ErrorMessage = "Joining Date is required.")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }

        public string? Photo { get; set; }
        public List<string>? Documents { get; set; }

        public IFormFile? PhotoFile { get; set; }
        public List<IFormFile>? NewDocuments { get; set; }
        public List<int>? RemoveDocIndexes { get; set; } = new List<int>();

        public bool EditMode { get; set; } = false;
        public bool IsStaff { get; set; } = false;
    }
}