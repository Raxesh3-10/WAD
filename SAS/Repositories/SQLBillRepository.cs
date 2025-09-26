using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SAS.Models;

namespace SAS.Repositories
{
    public class SQLBillRepository : IBillRepository
    {
        private readonly AppDbContext _context;

        public SQLBillRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Bill> GetAll()
        {
            return _context.Bills.ToList();
        }

        public Bill GetById(Guid id)
        {
            return _context.Bills.FirstOrDefault(b => b.Id == id);
        }

        public void Add(Bill bill)
        {
            _context.Bills.Add(bill);
            Save();
        }

        public void Update(Bill bill)
        {
            _context.Bills.Update(bill);
            Save();
        }

        public void Delete(Guid id)
        {
            var bill = GetById(id);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
                Save();
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}