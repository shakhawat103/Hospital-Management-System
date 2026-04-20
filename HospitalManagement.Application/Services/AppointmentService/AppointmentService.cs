using AutoMapper;
using HospitalManagement.Application.Services.AppointmentService;
using HospitalManagement.Core.DTOs.Appointments;
using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Enums;
using HospitalManagement.Application.Services.DoctorService;
using HospitalManagement.Core.Interfaces.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Application.Services.AppointmentService;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // === 👨‍⚕️ PATIENT: Browse Available Doctors ===
    public async Task<IEnumerable<DoctorCardDto>> GetAvailableDoctorsAsync()
    {
        // Get all active doctors (not deleted)
        var doctors = await _unitOfWork.Repository<Doctor>()
            .FindAsync(d => !d.IsDeleted);

        // Map to lightweight card DTOs
        return _mapper.Map<IEnumerable<DoctorCardDto>>(doctors);
    }

    // === 📅 PATIENT: Book a New Appointment ===
    public async Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto dto)
    {
        // 💡 Business Rule: Appointment date must be in future
        if (dto.AppointmentDate <= DateTime.Now)
            throw new InvalidOperationException("Appointment date must be in the future");

        // 💡 Business Rule: Doctor must exist and be active
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(dto.DoctorId);
        if (doctor == null || doctor.IsDeleted)
            throw new InvalidOperationException("Selected doctor is not available");

        // 💡 Business Rule: Patient must exist
        var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(dto.PatientId);
        if (patient == null || patient.IsDeleted)
            throw new InvalidOperationException("Patient not found");

        // 💡 Business Rule: Check for double-booking (same doctor, same time)
        var conflict = await _unitOfWork.Repository<Appointment>()
            .FindAsync(a => a.DoctorId == dto.DoctorId
                         && a.AppointmentDate == dto.AppointmentDate
                         && a.Status != AppointmentStatus.Cancelled);

        if (conflict.Any())
            throw new InvalidOperationException("This time slot is already booked");

        // Create new appointment
        var appointment = _mapper.Map<Appointment>(dto);
        appointment.Status = AppointmentStatus.Scheduled;

        await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Appointment booked: Patient {PatientId} with Doctor {DoctorId} on {Date}",
            dto.PatientId, dto.DoctorId, dto.AppointmentDate);

        return _mapper.Map<AppointmentDto>(appointment);
    }

    // === 📋 PATIENT: View Their Appointments ===
    public async Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(int patientId)
    {
        // ✅ FIX: Use Include to load Doctor data
        var appointments = await _unitOfWork.Repository<Appointment>()
            .GetAllAsync(); // Get all first

        // Filter and include doctor manually
        var patientAppointments = appointments
            .Where(a => a.PatientId == patientId && !a.IsDeleted)
            .ToList();

        // ✅ Manually load doctor for each appointment
        var dtos = new List<AppointmentDto>();
        foreach (var appt in patientAppointments)
        {
            // Add temporary logging to see what's happening:
            Console.WriteLine($"Appointment {appt.Id} - DoctorId: {appt.DoctorId}");

            // Load doctor explicitly
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(appt.DoctorId);

            if (doctor == null)
            {
                Console.WriteLine($"⚠️ Doctor {appt.DoctorId} NOT FOUND!");
            }
            else
            {
                Console.WriteLine($"✅ Doctor: {doctor.FirstName} {doctor.LastName}");
            }
            
            

            var dto = _mapper.Map<AppointmentDto>(appt);

            // ✅ Set doctor details
            if (doctor != null)
            {
                dto.DoctorName = $"{doctor.FirstName} {doctor.LastName}";
                dto.DoctorSpecialty = doctor.Specialty;
                dto.DoctorPhotoUrl = doctor.PhotoUrl;
                dto.DoctorFee = doctor.ConsultationFee;
            }

            dtos.Add(dto);
        }

        return dtos.OrderByDescending(a => a.AppointmentDate);
    }

    // === 🔍 GET BY ID (for doctors/admin) ===
    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
        if (appointment == null) return null;

        var dto = _mapper.Map<AppointmentDto>(appointment);

        // Enrich with doctor details
        if (appointment.Doctor != null)
        {
            dto.DoctorName = $"{appointment.Doctor.FirstName} {appointment.Doctor.LastName}";
            dto.DoctorSpecialty = appointment.Doctor.Specialty;
            dto.DoctorPhotoUrl = appointment.Doctor.PhotoUrl;
            dto.DoctorFee = appointment.Doctor.ConsultationFee;
        }

        return dto;
    }

    // === ❌ CANCEL APPOINTMENT ===
    public async Task<bool> CancelAppointmentAsync(int id, string reason)
    {
        var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
        if (appointment == null) return false;

        // 💡 Business Rule: Can't cancel already completed/cancelled appointments
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return false;

        // 💡 Business Rule: Can't cancel within 2 hours of appointment (optional)
        // if (appointment.AppointmentDate < DateTime.Now.AddHours(2))
        //     throw new InvalidOperationException("Cannot cancel within 2 hours of appointment");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.Notes = $"{appointment.Notes}\n[Cancelled: {reason}]";

        _unitOfWork.Repository<Appointment>().Update(appointment);
        var result = await _unitOfWork.SaveChangesAsync() > 0;

        if (result)
            _logger.LogInformation("Appointment {Id} cancelled: {Reason}", id, reason);

        return result;
    }

    // === ✅ MARK AS COMPLETED ===
    public async Task<bool> MarkAsCompletedAsync(int id)
    {
        var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
        if (appointment == null) return false;

        if (appointment.Status != AppointmentStatus.Scheduled)
            return false;  // Can only complete scheduled appointments

        appointment.Status = AppointmentStatus.Completed;
        _unitOfWork.Repository<Appointment>().Update(appointment);

        var result = await _unitOfWork.SaveChangesAsync() > 0;
        if (result)
            _logger.LogInformation("Appointment {Id} marked as completed", id);

        return result;
    }
}