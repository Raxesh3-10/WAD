using System.ComponentModel.DataAnnotations;

namespace SAS.ViewModels
{
    public class StudentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student name is required.")]
        public string StudentName { get; set; }

        [Required(ErrorMessage = "Father name is required.")]
        public string FatherName { get; set; }

        [Required(ErrorMessage = "Mother name is required.")]
        public string MotherName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Aadhar number is required.")]
        [Range(100000000000, 999999999999, ErrorMessage = "Aadhar number must be exactly 12 digits.")]
        public long AadharNo { get; set; }

        [Required(ErrorMessage = "Roll number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Roll number must be a positive number.")]
        public int RollNo { get; set; }

        [Required(ErrorMessage = "Division is required.")]
        [RegularExpression(@"^[A-Z]$", ErrorMessage = "Division must be a single uppercase letter (e.g. A, B, C).")]
        public string Div { get; set; }

        [Required(ErrorMessage = "Standard is required.")]
        [Range(1, 12, ErrorMessage = "Standard must be between 1 and 12.")]
        public int Std { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Range(1000000000, 9999999999, ErrorMessage = "Phone number must be 10 digits and cannot start with 0.")]
        public long PhoneNo { get; set; }
    }
}