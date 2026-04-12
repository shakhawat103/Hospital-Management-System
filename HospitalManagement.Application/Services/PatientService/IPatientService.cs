using HospitalManagement.Core.DTOs.Patients;
using System.Security.Claims;
namespace HospitalManagement.Application.Services.PatientService;
public interface IPatientService
{
    Task<PatientDto?> GetByIdAsync(int id);
    Task<IEnumerable<PatientDto>> GetAllAsync();
    Task<PatientDto> CreateAsync(CreatePatientDto dto);
    Task<bool> UpdateAsync(CreatePatientDto dto);  // ✅ Reuse same DTO for update
    Task<bool> DeleteAsync(int id);

    Task<int?> GetCurrentPatientIdAsync(ClaimsPrincipal user);
}