using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interface
{
    public interface IServiceBaseAsync<T> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task DeleteAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IPaginatedList<T>> GetAllPaginatedAsync(int pageIndex, int pageSize);
        Task<T> GetByIdAsync(int id);
        Task EditAsync(T entity);
        Task<bool> ExistsAsync(int id);
    }
}
