namespace HospitalManagement.Core.Entities;

// 💡 A Patient IS-A BaseEntity (gets Id, CreatedAt, IsDeleted for free)
public class Patient : BaseEntity
{
    // Required fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Optional: Link to ASP.NET Identity if you add login later
    // public string UserId { get; set; }
    // public ApplicationUser? User { get; set; }
}