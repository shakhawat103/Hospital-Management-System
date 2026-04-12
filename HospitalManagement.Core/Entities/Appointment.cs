using HospitalManagement.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Core.Entities;

public class Appointment : BaseEntity
{
    // === Links to Patient & Doctor ===
    public int PatientId { get; set; }
    public virtual Patient? Patient { get; set; }  // Navigation property

    public int DoctorId { get; set; }
    public virtual Doctor? Doctor { get; set; }    // Navigation property

    // === Appointment Details ===
    public DateTime AppointmentDate { get; set; }  // Date + time of visit
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }             // Patient's reason for visit
    public string? Diagnosis { get; set; }         // Doctor's notes (added later)

    // === Calculated Properties (for display) ===
    // These won't be stored in DB, just used in DTOs
    [NotMapped]
    public string DoctorName => Doctor?.FirstName + " " + Doctor?.LastName ?? "Unknown";

    [NotMapped]
    public string PatientName => Patient?.FirstName + " " + Patient?.LastName ?? "Unknown";
}