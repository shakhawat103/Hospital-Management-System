using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }  // ✅ NEW

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete filters
        modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Doctor>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted);

        // 💰 Decimal precision for money fields
        modelBuilder.Entity<Doctor>()
            .Property(d => d.ConsultationFee)
            .HasPrecision(18, 2);

        // 🔗 Appointment relationships (prevent cascade delete errors)
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)  // Requires: public ICollection<Appointment> Appointments { get; set; } in Patient
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);  // ✅ Prevent multiple cascade paths

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)  // Requires: public ICollection<Appointment> Appointments { get; set; } in Doctor
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}