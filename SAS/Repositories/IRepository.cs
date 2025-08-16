using System.Collections.Generic;

namespace SAS.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetByEmail(string email);
        void Add(T entity);
        bool Update(string email, T entity);
        bool Delete(string email);
    }
}
