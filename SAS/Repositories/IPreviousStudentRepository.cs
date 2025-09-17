using SAS.Models;
using System;
using System.Collections.Generic;

namespace SAS.Repositories
{
    public interface IPreviousStudentRepository
    {
        IEnumerable<PreviousStudent> GetAll();
        PreviousStudent GetById(Guid id);
        void Add(PreviousStudent student);
        void Update(PreviousStudent student);
        void Delete(Guid id);
    }
}