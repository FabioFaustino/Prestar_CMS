using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Prestar.Services
{
    /// <summary>
    /// This class uses the Skip and Take instructions to filter the data on the 
    /// server, instead of always retrieving all the rows in the table.
    /// </summary>
    public class PaginatedList<T> : List<T>
    {
        /// <summary>
        /// Property that gets and sets the page index
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// Property that gets and sets the number of pages
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Constructor of PaginatedList
        /// </summary>
        /// <param name="items">List of elements</param>
        /// <param name="count">Number of elements</param>
        /// <param name="pageIndex">Page Index</param>
        /// <param name="pageSize">Page Size</param>
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        /// <summary>
        /// Checks if a page has a previous page
        /// </summary>
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        /// <summary>
        /// Checks if a page has a next page
        /// </summary>
        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        /// <summary>
        /// Takes page size and page number and applies the appropriate Skip and 
        /// Take statements to the IQueryable.
        /// </summary>
        /// <param name="source">IQueryable source</param>
        /// <param name="pageIndex">Page Index</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public static async Task<PaginatedList<T>> CreateAsync(List<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count;
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
