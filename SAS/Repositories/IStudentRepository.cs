using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SAS.Models;

namespace SAS.Repositories
{
    public interface IStudentRepository
    {
        IEnumerable<Student> GetAll();
        Student? GetByEmail(string email);
        void Add(Student student, IFormFile? photoFile = null);
        bool Update(string email, Student updatedStudent, IFormFile? photoFile = null);
        bool Delete(string email);
    }
}