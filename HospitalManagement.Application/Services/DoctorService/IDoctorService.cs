using HospitalManagement.Core.Common;
using HospitalManagement.Core.DTOs.Doctors;



namespace HospitalManagement.Application.Services.DoctorService;

// 💡 Matches IPatientService pattern exactly
public interface IDoctorService
{
    Task<DoctorDto?> GetByIdAsync(int id);
    Task<PagedResult<DoctorDto>> GetPagedAsync(
    int pageNumber = 1,
    int pageSize = 10,
    string? searchTerm = null);
    Task<DoctorDto> CreateAsync(CreateDoctorDto dto);
    Task<bool> UpdateAsync(CreateDoctorDto dto);  // Reuse same DTO for update
    Task<bool> DeleteAsync(int id);
}