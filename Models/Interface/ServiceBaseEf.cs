using Microsoft.EntityFrameworkCore;
using MVC_LapinCouvert.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interface
{
    public class ServiceBaseEf
    {
        public class ServiceBaseEF<T> : IServiceBaseAsync<T> where T : class
        {
            protected readonly ApplicationDbContext _dbContext;

            public ServiceBaseEF(ApplicationDbContext dbContext) => _dbContext = dbContext;

            public virtual async Task<T> GetByIdAsync(int id)
            {
                return await _dbContext.Set<T>().FindAsync(id);
            }

            public virtual async Task<IReadOnlyList<T>> GetAllAsync()
            {
                return await _dbContext.Set<T>().ToListAsync();
            }

            public virtual async Task<IPaginatedList<T>> GetAllPaginatedAsync(int pageIndex, int pageSize)
            {

                return await _dbContext.Set<T>().ToPaginatedAsync(pageIndex, pageSize);
            }

            public virtual async Task<T> CreateAsync(T entity)
            {
                await _dbContext.Set<T>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();

                return entity;
            }

            public virtual async Task EditAsync(T entity)
            {
                if (_dbContext.Entry(entity).State == EntityState.Detached) _dbContext.Update(entity);
                else _dbContext.Entry(entity).State = EntityState.Modified;

                await _dbContext.SaveChangesAsync();
            }

            public virtual async Task DeleteAsync(int id)
            {
                var entity = await GetByIdAsync(id);
                _dbContext.Set<T>().Remove(entity);
                await _dbContext.SaveChangesAsync();
            }

            public async virtual Task<bool> ExistsAsync(int id)
            {
                return await GetByIdAsync(id) != null;
            }
        }
    }
}
