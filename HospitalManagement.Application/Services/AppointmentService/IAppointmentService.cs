using HospitalManagement.Core.DTOs.Appointments;

namespace HospitalManagement.Application.Services.AppointmentService;

public interface IAppointmentService
{
    // === For Patients: Browse & Book ===
    Task<IEnumerable<DoctorCardDto>> GetAvailableDoctorsAsync();
    Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto dto);

    // === For Patients: View Their Appointments ===
    Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(int patientId);

    // === For Doctors/Admin: Manage Appointments ===
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<bool> CancelAppointmentAsync(int id, string reason);
    Task<bool> MarkAsCompletedAsync(int id);
}