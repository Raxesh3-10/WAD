using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public class UserDetails
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public Guid UserId { get; set; }

        public User User { get; set; }

        public ICollection<UserSubject> Subjects { get; set; } = new List<UserSubject>();
        public ICollection<UserStd> Stds { get; set; } = new List<UserStd>();
        public ICollection<UserQualification> Qualifications { get; set; } = new List<UserQualification>();
        public ICollection<UserDocument> Documents { get; set; } = new List<UserDocument>();

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative.")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date, ErrorMessage = "DOB must be a valid date.")]
        public DateTime Dob { get; set; }

        [Url(ErrorMessage = "Photo must be a valid URL.")]
        public string Photo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Experience must be a non-negative integer.")]
        public int Experience { get; set; }

        [Required(ErrorMessage = "Joining Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Joining Date must be a valid date.")]
        public DateTime JoiningDate { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }
    }

    public class UserSubject
    {
        [Key]
        public int Id { get; set; }
        public string SubjectName { get; set; }

        public Guid UserDetailsId { get; set; }
        public UserDetails UserDetails { get; set; }
    }

    public class UserStd
    {
        [Key]
        public int Id { get; set; }
        public int Std { get; set; }

        public Guid UserDetailsId { get; set; }
        public UserDetails UserDetails { get; set; }
    }

    public class UserQualification
    {
        [Key]
        public int Id { get; set; }
        public string QualificationName { get; set; }

        public Guid UserDetailsId { get; set; }
        public UserDetails UserDetails { get; set; }
    }

    public class UserDocument
    {
        [Key]
        public int Id { get; set; }
        public string DocumentUrl { get; set; }

        public Guid UserDetailsId { get; set; }
        public UserDetails UserDetails { get; set; }
    }
}