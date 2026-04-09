using Microsoft.AspNetCore.Identity;

namespace HospitalManagement.Core.Entities.Identity;

// 💡 Extends IdentityUser to add hospital-specific fields
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}