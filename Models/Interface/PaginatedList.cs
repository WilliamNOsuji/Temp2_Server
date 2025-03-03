using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interface
{
    public class PaginatedList<T> : List<T>, IPaginatedList<T>
    {
        public int PageIndex { get; internal set; }
        public int PageSize { get; internal set; }
        public int TotalCount { get; internal set; }
        public int TotalPages { get; internal set; }

        public PaginatedList()
        {

        }

        public PaginatedList(int pageIndex, int pageSize, int totalCount, int totalPages, IEnumerable<T> items) : base(items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = totalPages;

        }

        public bool HasPreviousPage
        {
            get
            {
                return PageIndex > 0;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return PageIndex + 1 < TotalPages;
            }
        }
    }

    public static class PaginatedListExtensions
    {
        public async static Task<IPaginatedList<T>> ToPaginatedAsync<T>(this IEnumerable<T> list, int pageIndex, int pageSize)
        {
            var result = new PaginatedList<T>();
            var source = list.AsQueryable();
            result.PageIndex = pageIndex;
            result.PageSize = pageSize;
            result.TotalCount = await source.CountAsync();
            result.TotalPages = (int)Math.Ceiling(result.TotalCount / (double)result.PageSize);

            result.AddRange(await source.Skip(result.PageIndex * result.PageSize).Take(result.PageSize).ToListAsync());
            return result;
        }
        public static IPaginatedList<T> ToPaginated<T>(this IEnumerable<T> list, int pageIndex, int pageSize)
        {
            return list.ToPaginatedAsync(pageIndex, pageSize).Result;
        }
    }
}
