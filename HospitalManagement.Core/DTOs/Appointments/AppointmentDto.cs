using HospitalManagement.Core.Enums;

namespace HospitalManagement.Core.DTOs.Appointments;

// 💡 Data we SEND to views (for displaying appointments)
public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialty { get; set; } = string.Empty;
    public string? DoctorPhotoUrl { get; set; }
    public decimal DoctorFee { get; set; }

    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }

    // Helper: Format date for display
    public string FormattedDate => AppointmentDate.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt");

    // Helper: Status badge color
    public string StatusBadgeClass => Status switch
    {
        AppointmentStatus.Scheduled => "bg-primary",
        AppointmentStatus.Completed => "bg-success",
        AppointmentStatus.Cancelled => "bg-danger",
        AppointmentStatus.NoShow => "bg-warning text-dark",
        _ => "bg-secondary"
    };
}