using HospitalManagement.Core.DTOs.Doctors;


namespace HospitalManagement.Application.Services.DoctorService;

// 💡 Matches IPatientService pattern exactly
public interface IDoctorService
{
    Task<DoctorDto?> GetByIdAsync(int id);
    Task<IEnumerable<DoctorDto>> GetAllAsync();
    Task<DoctorDto> CreateAsync(CreateDoctorDto dto);
    Task<bool> UpdateAsync(CreateDoctorDto dto);  // Reuse same DTO for update
    Task<bool> DeleteAsync(int id);
}