namespace HospitalManagement.Core.DTOs.Doctors;

// 💡 Data we SEND to views (includes calculated fields for display)
public class DoctorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;  // Calculated: First + Last
    public string Specialty { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Add this property inside DoctorDto class:
    public string? PhotoUrl { get; set; }
}