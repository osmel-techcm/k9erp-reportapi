using System;
using System.Collections.Generic;
using System.Linq;

namespace reportCore.Entities
{
    public class Paginator<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> rows { get; set; }
        public List<FieldData> fields { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int? NextPageNumber => HasNextPage ? CurrentPage + 1 : (int?)null;
        public int? PreviousPageNumber => HasPreviousPage ? CurrentPage - 1 : (int?)null;

        public Paginator(List<T> items, PaginatorData paginatorData)
        {
            TotalCount = paginatorData.Count;
            PageSize = paginatorData.PageSize;
            CurrentPage = paginatorData.PageNumber;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            rows = new List<T>();
            rows.AddRange(items);
        }

        public static Paginator<T> Create(IEnumerable<T> source, PaginatorData paginatorData)
        {
            paginatorData.Count = source.Count();
            var totalPages = (int)Math.Ceiling(paginatorData.Count / (double)paginatorData.PageSize);

            if (paginatorData.PageSize > 100)
            {
                paginatorData.PageSize = 100;
            }

            if (paginatorData.PageNumber > totalPages)
            {
                paginatorData.PageNumber = totalPages;
            }

            if (paginatorData.PageNumber <= 0)
            {
                paginatorData.PageNumber = 1;
            }

            var resultList = source.Skip((paginatorData.PageNumber - 1) * paginatorData.PageSize).Take(paginatorData.PageSize).ToList();

            return new Paginator<T>(resultList, paginatorData);
        }
    }
}
