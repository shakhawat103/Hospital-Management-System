using AutoMapper;
using HospitalManagement.Core.DTOs.Appointments;
using HospitalManagement.Core.DTOs.Doctors;
using HospitalManagement.Core.DTOs.Patients;
using HospitalManagement.Core.Entities;

namespace HospitalManagement.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Create: DTO → Entity (ignore Id if present)
        CreateMap<CreatePatientDto, Patient>()
            .ForMember(d => d.Id, o => o.Ignore());

        // Update: DTO → Entity (only update if Id exists)
        CreateMap<CreatePatientDto, Patient>()
            .ForMember(d => d.Id, o => o.Condition(src => src.Id.HasValue))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
                srcMember != null && !srcMember.Equals(default)));

        CreateMap<Patient, CreatePatientDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.FullName, o => o.Ignore())  // Calculated field
            .ForMember(d => d.Age, o => o.Ignore())       // Calculated field
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt));

        // Read: Entity → DTO
        CreateMap<Patient, PatientDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.Age, o => o.MapFrom(s =>
                DateTime.Today.Year - s.DateOfBirth.Year -
                (DateTime.Today.DayOfYear < s.DateOfBirth.DayOfYear ? 1 : 0)));

        // === DOCTOR: CreateDoctorDto → Doctor (for saving) ===
        CreateMap<CreateDoctorDto, Doctor>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        // === DOCTOR: Doctor → DoctorDto (for displaying) ===
        CreateMap<Doctor, DoctorDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"));

        // === APPOINTMENT: CreateAppointmentDto → Appointment ===
        CreateMap<CreateAppointmentDto, Appointment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore());  // Set manually in service

        // === APPOINTMENT: Appointment → AppointmentDto ===
        // Inside MappingProfile class:
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                (s.Patient != null ? s.Patient.FirstName : string.Empty) + " " + (s.Patient != null ? s.Patient.LastName : string.Empty)))
            .ForMember(d => d.DoctorName, o => o.MapFrom(s =>
                (s.Doctor != null ? s.Doctor.FirstName : string.Empty) + " " + (s.Doctor != null ? s.Doctor.LastName : string.Empty)))
            .ForMember(d => d.DoctorSpecialty, o => o.MapFrom(s =>
                s.Doctor != null ? s.Doctor.Specialty : null))
            .ForMember(d => d.DoctorPhotoUrl, o => o.MapFrom(s =>
                s.Doctor != null ? s.Doctor.PhotoUrl : null))
            .ForMember(d => d.DoctorFee, o => o.MapFrom(s =>
                s.Doctor != null ? s.Doctor.ConsultationFee : 0));

        // === DOCTOR: Doctor → DoctorCardDto (for browsing) ===
        CreateMap<Doctor, DoctorCardDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.FormattedFee, o => o.Ignore());  // Calculated in DTO
    }
}