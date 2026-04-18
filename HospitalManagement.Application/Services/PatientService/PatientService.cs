// ✅ CORRECT USING STATEMENTS (no circular imports!)
using AutoMapper;
using HospitalManagement.Core; // ✅ Interface namespace
using HospitalManagement.Core.Common;
using HospitalManagement.Core.DTOs.Patients;
using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Interfaces.UnitOfWork;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

// ✅ CORRECT NAMESPACE (singular "Patient", not "PatientService")
namespace HospitalManagement.Application.Services.PatientService;

// 💡 This class contains ALL business logic for Patient operations
public class PatientService : IPatientService
{
    // === Dependencies (Injected by ASP.NET Core) ===
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PatientService> _logger;

    // Constructor: ASP.NET Core injects these automatically
    public PatientService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PatientService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ========================================================================
    // 🔍 READ OPERATIONS → Return PatientDto (for displaying to user)
    // ========================================================================

    /// <summary>
    /// Get one patient by ID
    /// Returns: CreatePatientDto (for display) or null if not found
    /// </summary>
    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        // 1. Fetch from database via generic repository
        var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(id);

        // 2. If not found, return null (controller handles 404)
        if (patient == null)
        {
            _logger.LogWarning("Patient with ID {Id} not found", id);
            return null;
        }

        // 3. Convert Entity → DTO using AutoMapper
        //    MappingProfile calculates FullName, Age automatically
        
        return _mapper.Map<PatientDto>(patient);
    }

    // GET FOR THE EDIT VIEW (same as GetById but returns CreatePatientDto for form population)
    public async Task<CreatePatientDto?> GetForEditAsync(int id)
    {
        // 1. Fetch from database via generic repository
        var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(id);

        // 2. If not found, return null (controller handles 404)
        if (patient == null)
        {
            _logger.LogWarning("Patient with ID {Id} not found", id);
            return null;
        }

        // 3. Convert Entity → DTO using AutoMapper
        //    MappingProfile calculates FullName, Age automatically

       
        //var dto = new CreatePatientDto
        //{
        //    Id = patient.Id,
        //    FirstName = patient.FirstName,
        //    LastName = patient.LastName,
        //    DateOfBirth = patient.DateOfBirth,
        //    Phone = patient.Phone,
        //    Email = patient.Email
        //};

        return _mapper.Map<CreatePatientDto>(patient);
    }

    /// <summary>

    // Replace GetAllAsync with this:

    public async Task<PagedResult<PatientDto>> GetPagedAsync(
    int pageNumber = 1,
    int pageSize = 10,
    string? searchTerm = null)
    {
        // 1. Get paged + filtered entities from repository
        var pagedPatients = await _unitOfWork.Repository<Patient>()
            .GetPagedAsync(pageNumber, pageSize, searchTerm);

        // 2. Map entities → DTOs
        var dtos = _mapper.Map<IEnumerable<PatientDto>>(pagedPatients.Items);

        // 3. Return new PagedResult with mapped DTOs + metadata
        return new PagedResult<PatientDto>
        {
            Items = dtos,
            PageNumber = pagedPatients.PageNumber,
            PageSize = pagedPatients.PageSize,
            TotalCount = pagedPatients.TotalCount
        };
    }

    // ========================================================================
    // ✍️ WRITE OPERATIONS → Accept CreatePatientDto (from form/API)
    // ========================================================================

    /// <summary>
    /// Create a NEW patient
    /// Input: CreatePatientDto (from form submission)
    /// Returns: PatientDto (with new Id populated)
    /// </summary>
    public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
    {
        // 💡 Business Rule: Email must be unique
        var existing = await _unitOfWork.Repository<Patient>()
            .FindAsync(p => p.Email == dto.Email);

        if (existing.Any())
        {
            _logger.LogWarning("Cannot create: email {Email} already exists", dto.Email);
            throw new InvalidOperationException($"Patient with email '{dto.Email}' already exists");
        }

        // 1. Convert DTO → Entity using AutoMapper
        var patient = _mapper.Map<Patient>(dto);

        // 2. Add to repository (marks as "new")
        await _unitOfWork.Repository<Patient>().AddAsync(patient);

        // 3. Save ALL changes to database (Unit of Work)
        await _unitOfWork.SaveChangesAsync();

        // 4. Log success
        _logger.LogInformation("Created patient {Id} with email {Email}", patient.Id, patient.Email);

        // 5. Return as DTO (with Id now populated from database)
        return _mapper.Map<PatientDto>(patient);
    }

    /// <summary>
    /// Update an EXISTING patient
    /// Input: CreatePatientDto (with Id populated for Edit)
    /// Returns: true if updated, false if not found
    /// </summary>
    public async Task<bool> UpdateAsync(CreatePatientDto dto)
    {
        // 💡 Safety: Id must exist for updates
        if (!dto.Id.HasValue)
        {
            _logger.LogWarning("Update failed: Id is null");
            return false;
        }

        // 1. Find existing patient
        var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(dto.Id.Value);
        if (patient == null)
        {
            _logger.LogWarning("Cannot update: patient {Id} not found", dto.Id);
            return false;
        }

        // 💡 Business Rule: Email must be unique (exclude current patient)
        var emailTaken = await _unitOfWork.Repository<Patient>()
            .FindAsync(p => p.Email == dto.Email && p.Id != dto.Id);

        if (emailTaken.Any())
        {
            _logger.LogWarning("Cannot update: email {Email} taken by another patient", dto.Email);
            throw new InvalidOperationException($"Email '{dto.Email}' is already registered");
        }

        // 2. Update fields manually (only the fields we allow to change)
        patient.FirstName = dto.FirstName;
        patient.LastName = dto.LastName;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.Phone = dto.Phone;
        patient.Email = dto.Email;
        // ❌ Don't touch: Id, CreatedAt, IsDeleted

        // 3. Mark as modified & save
        _unitOfWork.Repository<Patient>().Update(patient);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    // ========================================================================
    // 🗑️ DELETE OPERATION (Soft Delete)
    // ========================================================================

    /// <summary>
    /// Soft-delete a patient (sets IsDeleted = true)
    /// Returns: true if deleted, false if not found
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        // 1. Find the patient
        var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(id);
        if (patient == null)
        {
            _logger.LogWarning("Cannot delete: patient {Id} not found", id);
            return false;
        }

        // 2. SOFT DELETE: Mark as deleted instead of removing
        patient.IsDeleted = true;

        // 3. Mark as modified & save
        _unitOfWork.Repository<Patient>().Update(patient);
        var result = await _unitOfWork.SaveChangesAsync() > 0;

        // 4. Log result
        if (result)
            _logger.LogInformation("Soft-deleted patient {Id}", id);

        return result;
    }

    public async Task<int?> GetCurrentPatientIdAsync(ClaimsPrincipal user)
    {
        // 1. Check authentication safely
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        // 2. Extract email
        var email = user.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // 3. Query the repository
        // Use ToLower() to avoid casing mismatches
        var patients = await _unitOfWork.Repository<Patient>()
            .FindAsync(p => p.Email.ToLower() == email.ToLower());

        // 4. Extract the result
        var patient = patients?.FirstOrDefault();

        return patient?.Id;
    }

    // Inside PatientService.cs
    public async Task<Patient?> GetByUserIdAsync(string userId)
    {
        var patients = await _unitOfWork.Repository<Patient>()
            .FindAsync(p => p.UserId == userId);
        return patients.FirstOrDefault();
    }
}