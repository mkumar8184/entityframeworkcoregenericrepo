using BaseUtility.Utility;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericRepository.BaseRepository
{
    public class BaseRepository<T, TContext> : IBaseRepository<T, TContext> where T : BaseEntity where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly DbSet<T> _entities;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseRepository(TContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entities = _context.Set<T>();
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private void LogAndThrowException(string operation, Exception ex)
        {
            Log.Error($"DB Exception occurred while {operation}", ex);
            throw new Exception($"Oops, some internal error occurred during {operation}.", ex);
        }

        private async Task<int> SaveChangesAsync(IDbContextTransaction transaction = null)
        {
            try
            {
                // Set CompanyId for entities in the context before saving
                SetCompanyIdForEntities();

                var result = await _context.SaveChangesAsync();

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                DetachEntities();
                return result;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                LogAndThrowException("Save", ex);
                return 0; // unreachable due to the rethrow above, but required to satisfy method signature
            }
        }

        private void SetCompanyIdForEntities()
        {
            // Get the CompanyId from HttpContext (assumed to be set by middleware)
            var companyId = _httpContextAccessor.HttpContext?.Items["CompanyId"]?.ToString();

            if (string.IsNullOrEmpty(companyId))
            {
                throw new Exception("CompanyId is not available in the current context.");
            }

            // Convert to integer
            var companyIdInt = Convert.ToInt32(companyId);

            // Loop through all the tracked entities and set the CompanyId
            foreach (var entry in _context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.CompanyId = companyIdInt;
                }
            }
        }
     


        private void DetachEntities()
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }

        private void ChangeTrackingStrategy() => _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        #region CRUD Operations

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                
                 SetCompanyIdForEntity(entity,true);
                
                using var transaction = await _context.Database.BeginTransactionAsync();
                _entities.Add(entity);
                await SaveChangesAsync(transaction);
                return entity;
            }
            catch (Exception ex)
            {
                LogAndThrowException("inserting", ex);
                return null;
            }
        }

        public async Task<int> BulkInsertAsync(List<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            try
            {
                // Ensure CompanyId is set for all entities in the bulk insert
                foreach (var entity in entities)
                {
                    SetCompanyIdForEntity(entity,true);
                }  

                
                using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.BulkInsertAsync(entities);
                return await SaveChangesAsync(transaction);
            }
            catch (Exception ex)
            {
                LogAndThrowException("Bulk Insert", ex);
                return 0;
            }
        }

        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                SetCompanyIdForEntity(entity);

                using var transaction = await _context.Database.BeginTransactionAsync();
                _context.Entry(entity).State = EntityState.Modified;
                await SaveChangesAsync(transaction);
                return entity;
            }
            catch (Exception ex)
            {
                LogAndThrowException("updating", ex);
                return null;
            }
        }

        public async Task<int> BulkUpdateAsync(List<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            try
            {
                // Ensure CompanyId is set for all entities in the bulk update
                foreach (var entity in entities)
                {
                    SetCompanyIdForEntity(entity);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.BulkUpdateAsync(entities);
                return await SaveChangesAsync(transaction);
            }
            catch (Exception ex)
            {
                LogAndThrowException("Bulk Update", ex);
                return 0;
            }
        }

        public async Task<int> DeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                _entities.Remove(entity);
                return await SaveChangesAsync(transaction);
            }
            catch (Exception ex)
            {
                LogAndThrowException("deleting", ex);
                return 0;
            }
        }

        public async Task<int> BulkDeleteAsync(List<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.BulkDeleteAsync(entities);
                return await SaveChangesAsync(transaction);
            }
            catch (Exception ex)
            {
                LogAndThrowException("Bulk Delete", ex);
                return 0;
            }
        }

        #endregion

        #region Query Operations

        public async Task<T> FindByKeyAsync(object key)
        {
            try
            {
                var entity = await _entities.FindAsync(key);
                _context.Entry(entity).State = EntityState.Detached;
                return entity;
            }
            catch (Exception ex)
            {
                LogAndThrowException("FindByKey", ex);
                return null;
            }
        }

        public async Task<T> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _entities.FindAsync(id);
                ChangeTrackingStrategy();
                return entity;
            }
            catch (Exception ex)
            {
                LogAndThrowException("GetById", ex);
                return null;
            }
        }

        public IEnumerable<T> GetAll()
        {
            try
            {
                return _entities.AsNoTracking().AsEnumerable();
            }
            catch (Exception ex)
            {
                LogAndThrowException("GetAll", ex);
                return Enumerable.Empty<T>();
            }
        }

        public IEnumerable<TOut> Get<TIn, TOut>(IQuery<TIn, TOut> query) where TIn : class
        {
            try
            {
                ChangeTrackingStrategy();
                return query.Execute(_context.Set<TIn>().AsNoTracking()).AsEnumerable();
            }
            catch (Exception ex)
            {
                LogAndThrowException("Get", ex);
                return Enumerable.Empty<TOut>();
            }
        }

        public async Task<PaginatedResult<TOut>> Get<TIn, TOut>(IQuery<TIn, TOut> query, int pageIndex, int pageSize)
            where TIn : class
            where TOut : class
        {
            try
            {
                int skipItems = pageIndex * pageSize;

                // Get the paginated data
                var data = query.Execute(_context.Set<TIn>().AsNoTracking())
                                .Skip(skipItems)
                                .Take(pageSize)
                                .AsEnumerable();

                // Get the total count of records
                var totalCount = await query.Execute(_context.Set<TIn>().AsNoTracking()).CountAsync();

                return new PaginatedResult<TOut>
                {
                    Data = data,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                LogAndThrowException("Get with pagination", ex);
                return new PaginatedResult<TOut> { Data = Enumerable.Empty<TOut>(), TotalCount = 0 };
            }
        }

        public IEnumerable<T> ExecuteProcedure(string spName, params SqlParameter[] parameters)
        {
            try
            {
                ChangeTrackingStrategy();
                var result = _entities.FromSqlRaw($"EXEC {spName} {PrepareSqlParameters(parameters)}", parameters);
                return result;
            }
            catch (Exception ex)
            {
                LogAndThrowException("ExecuteProcedure", ex);
                return Enumerable.Empty<T>();
            }
        }

        public IEnumerable<T> ExecuteCustomQuery(string query)
        {
            try
            {
                ChangeTrackingStrategy();
                return _entities.FromSqlRaw(query);
            }
            catch (Exception ex)
            {
                LogAndThrowException("ExecuteCustomQuery", ex);
                return Enumerable.Empty<T>();
            }
        }

        #endregion

        private void SetCompanyIdForEntity(T entity,bool isCreating=false)
        {
            // Get the CompanyId from HttpContext (assumed to be set by middleware)
            var companyId = _httpContextAccessor.HttpContext?.Items["CompanyId"]?.ToString();
            var userId = _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();
            if (string.IsNullOrEmpty(companyId))
            {
                throw new Exception("CompanyId is not available in the current context.");
            }
            if(string.IsNullOrEmpty(userId))
            {
                throw new Exception("No user found to take actions in current context.");
            }

            // Convert to integer and set the CompanyId for the entity
            entity.CompanyId = Convert.ToInt32(companyId);
            entity.UpdatedBy = Convert.ToInt32(userId);
            if(isCreating)
            {
                entity.CreatedBy = Convert.ToInt32(userId);
            }
           

        }

        private string PrepareSqlParameters(SqlParameter[] parameters)
        {
            return string.Join(",", parameters.Select(p => p.ParameterName));
        }
    }
}
