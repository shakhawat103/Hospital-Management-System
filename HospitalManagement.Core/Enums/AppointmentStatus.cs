namespace HospitalManagement.Core.Enums;

public enum AppointmentStatus
{
    Scheduled,   // ✅ Upcoming
    Completed,   // ✅ Done
    Cancelled,   // ❌ Cancelled by patient/doctor
    NoShow       // ❌ Patient didn't attend
}