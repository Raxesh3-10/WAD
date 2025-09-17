using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SAS.Models;

namespace SAS.Repositories
{
    public interface INoticeRepository
    {
        IEnumerable<Notice> GetAll();
        Notice? GetByEmail(string email);
        void Add(Notice notice, List<IFormFile>? documents = null);
        bool Update(string email, Notice updatedNotice, List<IFormFile>? newDocuments = null);
        bool Delete(string email);
        bool DeleteById(Guid id);
    }
}