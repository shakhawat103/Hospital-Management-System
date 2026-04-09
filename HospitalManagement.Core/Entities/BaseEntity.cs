// 💡 All our entities inherit this to get free properties
namespace HospitalManagement.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }                    // Primary Key
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Auto-set when created
    public bool IsDeleted { get; set; }            // Soft delete (instead of actually deleting)
}