using HospitalManagement.Core.DTOs.Patients;

public interface IPatientService
{
    Task<PatientDto?> GetByIdAsync(int id);
    Task<IEnumerable<PatientDto>> GetAllAsync();
    Task<PatientDto> CreateAsync(CreatePatientDto dto);
    Task<bool> UpdateAsync(CreatePatientDto dto);  // ✅ Reuse same DTO for update
    Task<bool> DeleteAsync(int id);
}