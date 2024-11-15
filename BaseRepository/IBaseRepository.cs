using BaseUtility.TableSearchUtil;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericRepository.BaseRepository
{
    public interface IBaseRepository<T, TContext> where T : BaseEntity where TContext : DbContext
    {
        #region CRUD Operations

        Task<T> AddAsync(T entity);
        Task<int> BulkInsertAsync(List<T> entities);
        Task<T> UpdateAsync(T entity);
        Task<int> BulkUpdateAsync(List<T> entities);
        Task<int> DeleteAsync(T entity);
        Task<int> BulkDeleteAsync(List<T> entities);

        #endregion

        #region Query Operations

        Task<T> FindByKeyAsync(object key);
        Task<T> GetByIdAsync(int id);
        IEnumerable<T> GetAll();
        IEnumerable<TOut> Get<TIn, TOut>(IQuery<TIn, TOut> query) where TIn : class;

        // Use PaginatedResult<TOut> for clearer structure
        Task<PaginatedResult<TOut>> Get<TIn, TOut>(IQuery<TIn, TOut> query, int pageIndex, int pageSize)
            where TIn : class
            where TOut : class;

        IEnumerable<T> ExecuteProcedure(string spName, params SqlParameter[] parameters);
        IEnumerable<T> ExecuteCustomQuery(string query);

        #endregion
    }

    // Paginated result class to return both data and total count
    public class PaginatedResult<TOut>
    {
        public IEnumerable<TOut> Data { get; set; }
        public int TotalCount { get; set; }
    }
}




