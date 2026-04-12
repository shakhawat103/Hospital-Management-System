namespace HospitalManagement.Core.DTOs.Appointments;

// 💡 Lightweight DTO for displaying doctors in a card/list
public class DoctorCardDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public decimal ConsultationFee { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Helper: Format fee for display
    public string FormattedFee => ConsultationFee.ToString("C");
}