using AutoMapper;
using HospitalManagement.Application.Services.DoctorService;
using HospitalManagement.Core.Common;
using HospitalManagement.Core.DTOs.Doctors;
using HospitalManagement.Core.DTOs.Patients;
using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Interfaces.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Application.Services.DoctorService;

// 💡 Same structure as PatientService - just swap Patient → Doctor
public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DoctorService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ========================================================================
    // 🔍 READ OPERATIONS → Return DoctorDto (for displaying)
    // ========================================================================

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id);
        if (doctor == null)
        {
            _logger.LogWarning("Doctor with ID {Id} not found", id);
            return null;
        }

        return _mapper.Map<DoctorDto>(doctor);
    }

    public async Task<PagedResult<DoctorDto>> GetPagedAsync(
    int pageNumber = 1,
    int pageSize = 10,
    string? searchTerm = null)
    {
        // 1. Get paged + filtered entities from repository
        var pagedPatients = await _unitOfWork.Repository<Doctor>()
            .GetPagedAsync(pageNumber, pageSize, searchTerm);

        // 2. Map entities → DTOs
        var dtos = _mapper.Map<IEnumerable<DoctorDto>>(pagedPatients.Items);

        // 3. Return new PagedResult with mapped DTOs + metadata
        return new PagedResult<DoctorDto>
        {
            Items = dtos,
            PageNumber = pagedPatients.PageNumber,
            PageSize = pagedPatients.PageSize,
            TotalCount = pagedPatients.TotalCount
        };
    }

    // ========================================================================
    // ✍️ WRITE OPERATIONS → Accept CreateDoctorDto (from form)
    // ========================================================================

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
    {
        // 💡 Business Rule: License number must be unique
        var existing = await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.LicenseNumber == dto.LicenseNumber);

        if (existing.Any())
        {
            _logger.LogWarning("Cannot create: license {License} already exists", dto.LicenseNumber);
            throw new InvalidOperationException($"Doctor with license '{dto.LicenseNumber}' already exists");
        }

        // 💡 Business Rule: Email must be unique
        var emailTaken = await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.Email == dto.Email);

        if (emailTaken.Any())
        {
            _logger.LogWarning("Cannot create: email {Email} already exists", dto.Email);
            throw new InvalidOperationException($"Doctor with email '{dto.Email}' already exists");
        }

        // 1. Convert DTO → Entity
        var doctor = _mapper.Map<Doctor>(dto);

        // 2. Add to repository
        await _unitOfWork.Repository<Doctor>().AddAsync(doctor);

        // 3. Save to database
        await _unitOfWork.SaveChangesAsync();

        // 4. Log & return
        _logger.LogInformation("Created doctor {Id} with license {License}", doctor.Id, doctor.LicenseNumber);
        return _mapper.Map<DoctorDto>(doctor);
    }

    public async Task<bool> UpdateAsync(CreateDoctorDto dto)
    {
        if (!dto.Id.HasValue)
        {
            _logger.LogWarning("Update failed: Id is null");
            return false;
        }

        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(dto.Id.Value);
        if (doctor == null)
        {
            _logger.LogWarning("Cannot update: doctor {Id} not found", dto.Id);
            return false;
        }

        // 💡 Business Rule: License must be unique (exclude current doctor)
        var licenseTaken = await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.LicenseNumber == dto.LicenseNumber && d.Id != dto.Id);

        if (licenseTaken.Any())
        {
            _logger.LogWarning("Cannot update: license {License} taken by another doctor", dto.LicenseNumber);
            throw new InvalidOperationException($"License '{dto.LicenseNumber}' is already registered");
        }

        // 💡 Business Rule: Email must be unique (exclude current doctor)
        var emailTaken = await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => d.Email == dto.Email && d.Id != dto.Id);

        if (emailTaken.Any())
        {
            _logger.LogWarning("Cannot update: email {Email} taken by another doctor", dto.Email);
            throw new InvalidOperationException($"Email '{dto.Email}' is already registered");
        }

        // Update fields
        doctor.FirstName = dto.FirstName;
        doctor.LastName = dto.LastName;
        doctor.Specialty = dto.Specialty;
        doctor.LicenseNumber = dto.LicenseNumber;
        doctor.ConsultationFee = dto.ConsultationFee;
        doctor.Phone = dto.Phone;
        doctor.Email = dto.Email;

        _unitOfWork.Repository<Doctor>().Update(doctor);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    // ========================================================================
    // 🗑️ DELETE OPERATION (Soft Delete)
    // ========================================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id);
        if (doctor == null)
        {
            _logger.LogWarning("Cannot delete: doctor {Id} not found", id);
            return false;
        }

        // Soft delete
        doctor.IsDeleted = true;
        _unitOfWork.Repository<Doctor>().Update(doctor);

        var result = await _unitOfWork.SaveChangesAsync() > 0;
        if (result)
            _logger.LogInformation("Soft-deleted doctor {Id}", id);

        return result;
    }
}