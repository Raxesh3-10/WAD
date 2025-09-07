using System;
using System.Collections.Generic;
using System.Linq;
using SAS.Models;
using Microsoft.EntityFrameworkCore;

namespace SAS.Repositories
{
    public class SQLStudentRepository : IRepository<Student>
    {
        private readonly AppDbContext _context;

        public SQLStudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Student> GetAll()
        {
            return _context.Students.ToList();
        }

        public Student? GetByEmail(string email)
        {
            return _context.Students.FirstOrDefault(s => s.Email == email);
        }

        public void Add(Student student)
        {
            if (student.Id == Guid.Empty)
                student.Id = Guid.NewGuid();

            _context.Students.Add(student);
            _context.SaveChanges();
        }

        public bool Update(string email, Student updatedStudent)
        {
            var existing = _context.Students.FirstOrDefault(s => s.Email == email);
            if (existing == null) return false;

            updatedStudent.Id = existing.Id;

            _context.Entry(existing).CurrentValues.SetValues(updatedStudent);
            _context.SaveChanges();
            return true;
        }

        public bool Delete(string email)
        {
            var student = _context.Students.FirstOrDefault(s => s.Email == email);
            if (student == null) return false;

            _context.Students.Remove(student);
            _context.SaveChanges();
            return true;
        }
    }
}