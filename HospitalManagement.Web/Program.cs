using HospitalManagement.Application.Mapping;

using HospitalManagement.Application.Services.PatientService;
using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Entities.Identity;
using HospitalManagement.Core.Enums;
using HospitalManagement.Core.Interfaces.UnitOfWork;
using HospitalManagement.Infrastructure;
using HospitalManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC
builder.Services.AddControllersWithViews();

// 2. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. IDENTITY CONFIGURATION ✅
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// === 3. Add AutoMapper ===
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));  // Finds MappingProfile automatically

// 5. DI Registration
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPatientService, PatientService>();

var app = builder.Build();

// Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // ✅ Required for Identity
app.UseAuthorization();  // ✅ Required for [Authorize]

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// 🌱 SEED ROLES & DEFAULT ADMIN ✅
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Create roles
        foreach (var role in new[] { UserRole.Admin, UserRole.Doctor, UserRole.Patient })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Create default admin if missing
        var adminEmail = "admin@hospital.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123!"); // ⚠️ CHANGE IN PRODUCTION
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, UserRole.Admin);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding Identity roles/users.");
    }
}

app.Run();