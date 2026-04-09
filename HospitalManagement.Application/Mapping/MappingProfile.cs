using AutoMapper;
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

        // Read: Entity → DTO
        CreateMap<Patient, PatientDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.Age, o => o.MapFrom(s =>
                DateTime.Today.Year - s.DateOfBirth.Year -
                (DateTime.Today.DayOfYear < s.DateOfBirth.DayOfYear ? 1 : 0)));
    }
}