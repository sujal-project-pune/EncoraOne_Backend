using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EncoraOne.Grievance.API.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Get all records
        Task<IEnumerable<T>> GetAllAsync();

        // Get a single record by ID
        Task<T> GetByIdAsync(int id);

        // Find records based on a condition (e.g., Get all complaints where Status == Pending)
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Add a new record
        Task AddAsync(T entity);

        // Remove a record
        void Remove(T entity);

        // Note: Update is often handled by EF Core tracking, but we can expose a method if needed.
        void Update(T entity);
    }
}