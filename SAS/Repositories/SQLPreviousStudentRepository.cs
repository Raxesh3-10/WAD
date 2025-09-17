using SAS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAS.Repositories
{
    public class SQLPreviousStudentRepository : IPreviousStudentRepository
    {
        private readonly AppDbContext _context;

        public SQLPreviousStudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<PreviousStudent> GetAll()
        {
            return _context.PreviousStudents.ToList();
        }

        public PreviousStudent GetById(Guid id)
        {
            return _context.PreviousStudents.Find(id);
        }

        public void Add(PreviousStudent student)
        {
            _context.PreviousStudents.Add(student);
            _context.SaveChanges();
        }

        public void Update(PreviousStudent student)
        {
            _context.PreviousStudents.Update(student);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var student = _context.PreviousStudents.Find(id);
            if (student != null)
            {
                _context.PreviousStudents.Remove(student);
                _context.SaveChanges();
            }
        }
    }
}