using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int pageNumber = 0, int pageSize = 0);
        Task<T> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);
        Task CreateAsync(T entity);
        Task<bool> Delete(int id);
    }
}
