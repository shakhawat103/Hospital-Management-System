namespace HospitalManagement.Core.DTOs.Patients;

// 💡 Used for displaying patients (Index, Details, Edit pre-fill)
public class PatientDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;  // Calculated: First + Last
    public DateTime DateOfBirth { get; set; }              // ✅ Added for Edit form
    public int Age { get; set; }                           // Calculated from DOB
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}