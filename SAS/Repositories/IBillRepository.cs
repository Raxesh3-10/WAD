using SAS.Models;
using System;
using System.Collections.Generic;

namespace SAS.Repositories
{
    public interface IBillRepository
    {
        IEnumerable<Bill> GetAll();
        Bill GetById(Guid id);
        void Add(Bill bill);
        void Update(Bill bill);
        void Delete(Guid id);
        void Save();
    }
}