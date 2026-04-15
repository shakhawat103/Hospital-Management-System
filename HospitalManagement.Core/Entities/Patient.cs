using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Entities.Identity;

public class Patient : BaseEntity
{
    // ✅ Link to ASP.NET Core Identity User (Only ONE UserId property!)
    public string? UserId { get; set; } 
    public virtual ApplicationUser? User { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public virtual ICollection<Appointment>? Appointments { get; set; } = new List<Appointment>();
}