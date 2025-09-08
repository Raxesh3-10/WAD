using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SAS.ViewModels
{
    public class UserDetailsViewModel
    {
        public string UserEmail { get; set; } = string.Empty;

        public List<string>? Subjects { get; set; }
        public List<int>? Std { get; set; }
        public List<string>? Qualifications { get; set; }

        public string? SubjectsText { get; set; }
        public string? StdText { get; set; }
        public string? QualificationsText { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        public decimal? Salary { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime Dob { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Experience must be non-negative.")]
        public int? Experience { get; set; }

        [Required(ErrorMessage = "Joining date is required.")]
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
