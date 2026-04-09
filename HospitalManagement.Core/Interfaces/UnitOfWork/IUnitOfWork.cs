// 💡 Unit of Work: "I coordinate saving ALL changes together"
// Like a transaction: either everything saves, or nothing does.

using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Interfaces.Repositories;

namespace HospitalManagement.Core.Interfaces.UnitOfWork;

public interface IUnitOfWork : IDisposable  // IDisposable so we can "using var uow = ..."
{
    // Get a repository for any entity
    IRepository<T> Repository<T>() where T : BaseEntity;

    // Save ALL pending changes (from any repository) in one go
    Task<int> SaveChangesAsync();
}