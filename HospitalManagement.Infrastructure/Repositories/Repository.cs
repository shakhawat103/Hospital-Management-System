using HospitalManagement.Core.Common;
using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Interfaces.Repositories;
using HospitalManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HospitalManagement.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    // ✅ Search with pagination (main method for list views)
    public async Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _dbSet.AsNoTracking().AsQueryable();

        // 🔍 Apply search filter if term provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower().Trim();

            // 💡 Generic search: works for any entity with FirstName/LastName/Email/Phone
            // Uses reflection to find searchable string properties
            query = query.Where(e =>
                EF.Property<string>(e, "FirstName").Contains(searchTerm) ||
                EF.Property<string>(e, "LastName").Contains(searchTerm) ||
                EF.Property<string>(e, "Email").Contains(searchTerm) ||
                EF.Property<string>(e, "Phone").Contains(searchTerm)
            );
        }

        return await PagedResult<T>.CreateAsync(query, pageNumber, pageSize);
    }

    // ✅ Simple search for dropdowns/autocomplete
    public async Task<IEnumerable<T>> SearchAsync(string searchTerm, int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<T>();

        searchTerm = searchTerm.ToLower().Trim();

        return await _dbSet.AsNoTracking()
            .Where(e =>
                EF.Property<string>(e, "FirstName").Contains(searchTerm) ||
                EF.Property<string>(e, "LastName").Contains(searchTerm) ||
                EF.Property<string>(e, "Email").Contains(searchTerm) ||
                EF.Property<string>(e, "Phone").Contains(searchTerm)
            )
            .Take(maxResults)
            .ToListAsync();
    }
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        _dbSet.Update(entity);
    }
}