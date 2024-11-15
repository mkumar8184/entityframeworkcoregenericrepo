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


    //public class UserProfileViewRepository : BaseRepository<UserProfileView, PSIDbContext>, IUserViewProfileRepository
    //{
    //    public UserProfileViewRepository(PSIDbContext context) : base(context)
    //    {
    //    }
    //}

    //public async Task<bool> AddOrderWithDetailsAsync(Order order, List<OrderDetail> orderDetails)
    //{
    //    if (order == null || orderDetails == null || orderDetails.Count == 0)
    //        throw new ArgumentNullException("Order and OrderDetails cannot be null");

    //    try
    //    {
    //        // Start a new transaction
    //        using var transaction = await _context.Database.BeginTransactionAsync();

    //        // Add the order
    //        _context.Set<Order>().Add(order);
    //        await SaveChangesAsync(transaction);  // Save Order to generate its Id

    //        // Now, set the foreign key in orderDetails and add them
    //        foreach (var detail in orderDetails)
    //        {
    //            detail.OrderId = order.Id; // Link OrderDetail to the created Order
    //            _context.Set<OrderDetail>().Add(detail);
    //        }

    //        // Save changes for the OrderDetails
    //        await SaveChangesAsync(transaction);

    //        // Commit the transaction
    //        await transaction.CommitAsync();

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        // Rollback the transaction if an error occurs
    //        LogAndThrowException("Saving order with details", ex);
    //        return false;
    //    }
    //}


