using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SAS.Models;
using SAS.Services;

namespace SAS.Repositories
{
    public class SQLStudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public SQLStudentRepository(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public IEnumerable<Student> GetAll()
        {
            return _context.Students.ToList();
        }

        public Student? GetByEmail(string email)
        {
            return _context.Students.FirstOrDefault(s => s.Email == email);
        }

        public void Add(Student student, IFormFile? photoFile = null)
        {
            if (student.Id == Guid.Empty)
                student.Id = Guid.NewGuid();

            if (photoFile != null)
            {
                student.PhotoUrl = _cloudinaryService.UploadImage(photoFile, "documents");
            }

            _context.Students.Add(student);
            _context.SaveChanges();
        }

        public bool Update(string email, Student updatedStudent, IFormFile? photoFile = null)
        {
            var existing = _context.Students.FirstOrDefault(s => s.Email == email);
            if (existing == null) return false;

            updatedStudent.Id = existing.Id;

            if (photoFile != null)
            {
                if (!string.IsNullOrEmpty(existing.PhotoUrl))
                {
                    _cloudinaryService.DeleteFromCloudinary(existing.PhotoUrl, true);
                }

                updatedStudent.PhotoUrl = _cloudinaryService.UploadImage(photoFile, "documents");
            }
            else
            {
                updatedStudent.PhotoUrl = existing.PhotoUrl; 
            }

            _context.Entry(existing).CurrentValues.SetValues(updatedStudent);
            _context.SaveChanges();
            return true;
        }

        public bool Delete(string email)
        {
            var student = _context.Students.FirstOrDefault(s => s.Email == email);
            if (student == null) return false;

            if (!string.IsNullOrEmpty(student.PhotoUrl))
            {
                _cloudinaryService.DeleteFromCloudinary(student.PhotoUrl, true);
            }

            _context.Students.Remove(student);
            _context.SaveChanges();
            return true;
        }
    }
}