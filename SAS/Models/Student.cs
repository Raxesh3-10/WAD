using System;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public class Student
    {
        public Guid Id { get; set; }
        
        public string StudentName { get; set; }
        public string? PhotoUrl {  get; set; }

        public string FatherName { get; set; }
        
        public string MotherName { get; set; }
        
        public string Email { get; set; }
        
        public long AadharNo { get; set; }
        
        public int RollNo { get; set; }
        
        public string Div { get; set; }
        
        public int Std { get; set; }
        
        public long PhoneNo { get; set; }
    }
}