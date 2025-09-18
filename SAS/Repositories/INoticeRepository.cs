using System;
using System.Collections.Generic;
using SAS.Models;

namespace SAS.Repositories
{
    public interface INoticeRepository
    {
        IEnumerable<Notice> GetAll();
        Notice? GetByEmail(string email);
        Notice? GetById(Guid id);
        void Add(Notice notice);
        bool Update(Notice notice);
        bool DeleteById(Guid id);
    }
}