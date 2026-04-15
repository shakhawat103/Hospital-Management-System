// 💡 Generic Repository: "I can work with ANY entity that inherits BaseEntity"
// <T> is a placeholder. When we use IRepository<Patient>, T becomes Patient.

using HospitalManagement.Core.Common;
using HospitalManagement.Core.Entities;
using System.Linq.Expressions;  // For filtering: "Find patients where Email contains 'gmail'"

namespace HospitalManagement.Core.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    // READ operations
    Task<T?> GetByIdAsync(int id);                    // Get one by ID
    Task<IEnumerable<T>> GetAllAsync();               // Get all
                                                      // Add this method inside the interface:

    // Search with pagination (for list views)
    Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null);

    // Search without pagination (for dropdowns, autocomplete)
    Task<IEnumerable<T>> SearchAsync(string searchTerm, int maxResults = 50);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);  // Get by condition

    // WRITE operations  
    Task AddAsync(T entity);                          // Insert new
    void Update(T entity);                            // Update existing
    void Delete(T entity);                            // Mark as deleted (soft delete)
}