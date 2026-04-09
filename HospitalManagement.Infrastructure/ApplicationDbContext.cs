using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure;

// 💡 Inherit IdentityDbContext instead of DbContext
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // ⚠️ MUST call this first!

        modelBuilder.Entity<Patient>()
            .HasQueryFilter(p => !p.IsDeleted);
    }
}