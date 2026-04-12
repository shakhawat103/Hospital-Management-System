using HospitalManagement.Core.Entities;

public class Doctor : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // ✅ NEW: Profile photo URL (stored as string path/URL)
    public string? PhotoUrl { get; set; }  // Nullable, optional

    // Optional: Link to Identity user later
    // public string UserId { get; set; }
    // public ApplicationUser? User { get; set; }

    // Navigation: One patient has many appointments
    public virtual ICollection<Appointment>? Appointments { get; set; } = new List<Appointment>();
}