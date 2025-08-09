using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public class Student
    {
        public int Id { get; set; }
        [Required]
        public string StudentName { get; set; }

        [Required]
        public string FatherName { get; set; }

        [Required]
        public string MotherName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required]
        [Range(100000000000, 999999999999, ErrorMessage = "Aadhar number must be exactly 12 digits.")]
        public long AadharNo { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Roll number must be a positive number.")]
        public int RollNo { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]$", ErrorMessage = "Division must be a single uppercase letter (e.g. A, B, C).")]
        public string Div { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Standard must be between 1 and 12.")]
        public int Std { get; set; }

        [Required]
        [Range(1000000000, 9999999999, ErrorMessage = "Phone number must be 10 digits and cannot start with 0.")]
        public long PhoneNo { get; set; }

        // Optional: Constructor with validation logic if you're not using model binding
        public Student() { }

        public Student(string studentName, string fatherName, string motherName, string email,
                       long aadharNo, int rollNo, string div, int std, long phoneNo)
        {
            StudentName = studentName;
            FatherName = fatherName;
            MotherName = motherName;
            Email = email;
            AadharNo = aadharNo;
            RollNo = rollNo;
            Div = div.ToUpper();
            Std = std;
            PhoneNo = phoneNo;
        }

        public object GetProfile()
        {
            return new
            {
                StudentName,
                FatherName,
                MotherName,
                Email,
                Std,
                Div,
                RollNo,
                PhoneNo,
                AadharNo
            };
        }
    }
}
