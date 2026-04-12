using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;  // ✅ For IFormFile

namespace HospitalManagement.Core.DTOs.Doctors;

public class CreateDoctorDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialty is required")]
    public string Specialty { get; set; } = string.Empty;

    [Required(ErrorMessage = "License number is required")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [Range(0, 99999.99, ErrorMessage = "Fee must be between 0 and 99,999.99")]
    public decimal ConsultationFee { get; set; }

    [Required]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;

    // ✅ NEW: File upload field (only used in forms, ignored by AutoMapper)
    [Display(Name = "Profile Photo")]
    public IFormFile? PhotoFile { get; set; }

    // ✅ URL saved to database after upload
    public string? PhotoUrl { get; set; }
}