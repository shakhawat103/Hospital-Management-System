using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Core.DTOs.Appointments;

// 💡 Data we ACCEPT when booking an appointment (from patient form)
public class CreateAppointmentDto
{
    public int? Id { get; set; }  // For edits (optional)

    [Required]
    public int PatientId { get; set; }  // Hidden field: which patient is booking

    [Required(ErrorMessage = "Please select a doctor")]
    public int DoctorId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [FutureDate(ErrorMessage = "Appointment date must be in the future")]
    public DateTime AppointmentDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }  // Reason for visit
}

// 💡 Custom validation attribute: Date must be in future
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date && date > DateTime.Now)
            return ValidationResult.Success;

        return new ValidationResult("Appointment date must be in the future");
    }
}