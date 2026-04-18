using HospitalManagement.Core.Common;
using HospitalManagement.Core.DTOs.Patients;
using System.Security.Claims;
namespace HospitalManagement.Application.Services.PatientService;
public interface IPatientService
{
    Task<PatientDto?> GetByIdAsync(int id);
    Task<CreatePatientDto?> GetForEditAsync(int id);

    // Replace GetAllAsync with this:
    Task<PagedResult<PatientDto>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null);
    Task<PatientDto> CreateAsync(CreatePatientDto dto);
    Task<bool> UpdateAsync(CreatePatientDto dto);  // ✅ Reuse same DTO for update
    Task<bool> DeleteAsync(int id);

    Task<int?> GetCurrentPatientIdAsync(ClaimsPrincipal user);

    Task<Patient?> GetByUserIdAsync(string userId);
}