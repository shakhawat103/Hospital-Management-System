using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Core.DTOs.Patients;

// 💡 This DTO now handles: Create, Edit, AND Display (Details/Delete confirmation)
// We add optional display-only fields that won't break forms
public class CreatePatientDto
{
    // === Form Fields (for Create/Edit) ===

    // Optional: Only used for Edit/Details (null when creating new)
    public int? Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;

    // === Display-Only Fields (for Details/Delete) ===
    // These are ignored by forms but useful for showing info

    // Calculated: First + Last name (set by controller/service)
    public string FullName { get; set; } = string.Empty;

    // Calculated: Age in years (set by controller/service)
    public int Age { get; set; }

    // When patient was registered (set by service on create)
    public DateTime? CreatedAt { get; set; }
}