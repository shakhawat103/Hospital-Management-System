// 💡 Generic Repository: "I can work with ANY entity that inherits BaseEntity"
// <T> is a placeholder. When we use IRepository<Patient>, T becomes Patient.

using System.Linq.Expressions;  // For filtering: "Find patients where Email contains 'gmail'"
using HospitalManagement.Core.Entities;

namespace HospitalManagement.Core.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    // READ operations
    Task<T?> GetByIdAsync(int id);                    // Get one by ID
    Task<IEnumerable<T>> GetAllAsync();               // Get all
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);  // Get by condition

    // WRITE operations  
    Task AddAsync(T entity);                          // Insert new
    void Update(T entity);                            // Update existing
    void Delete(T entity);                            // Mark as deleted (soft delete)
}