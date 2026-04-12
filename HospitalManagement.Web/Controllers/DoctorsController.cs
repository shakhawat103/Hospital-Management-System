using HospitalManagement.Application.Services.DoctorService;
using HospitalManagement.Core.DTOs.Doctors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize(Roles = "Admin")]
public class DoctorsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IWebHostEnvironment _webHost;  // ✅ NEW

    public DoctorsController(IDoctorService doctorService, IWebHostEnvironment webHost)
    {
        _doctorService = doctorService;
        _webHost = webHost;
    }

    
    // 💡 Handles file upload & returns the relative URL
    private async Task<string?> UploadPhotoAsync(IFormFile? file, string? existingUrl = null)
    {
        if (file == null || file.Length == 0) return existingUrl;

        // Only allow images
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(ext))
            throw new InvalidOperationException("Only JPG, PNG, or WEBP images are allowed");

        // Save to wwwroot/images/doctors/
        var uploadsDir = Path.Combine(_webHost.WebRootPath, "images", "doctors");
        Directory.CreateDirectory(uploadsDir);

        // Generate unique filename
        var uniqueName = Guid.NewGuid().ToString("N") + ext;
        var filePath = Path.Combine(uploadsDir, uniqueName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/doctors/{uniqueName}";
    }

    // 📋 List all doctors
    public async Task<IActionResult> Index()
    {
        var doctors = await _doctorService.GetAllAsync();
        return View(doctors);
    }

    // 👁️ View one doctor
    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        return doctor != null ? View(doctor) : NotFound();
    }

    // ➕ Show create form
    public IActionResult Create() => View(new CreateDoctorDto());

    // ➕ Handle create submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDoctorDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            // ✅ Upload photo if provided
            dto.PhotoUrl = await UploadPhotoAsync(dto.PhotoFile);

            await _doctorService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    // ✏️ Show edit form (reuses Create view)
    // ✏️ GET: /Doctors/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound();

        // Map DoctorDto → CreateDoctorDto for the edit form
        var nameParts = doctor.FullName.Split(' ', 2);
        var dto = new CreateDoctorDto
        {
            Id = doctor.Id,
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : "",
            Specialty = doctor.Specialty,
            LicenseNumber = doctor.LicenseNumber,
            ConsultationFee = doctor.ConsultationFee,
            Phone = doctor.Phone,
            Email = doctor.Email,
            PhotoUrl = doctor.PhotoUrl  // ✅ Pass existing photo URL to view
        };

        return View(dto);  // Automatically finds Edit.cshtml
    }

    // ✏️ Handle edit submission
    // ✏️ POST: /Doctors/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CreateDoctorDto dto)
    {
        if (!ModelState.IsValid) return View(dto);  // ✅ Returns Edit.cshtml
        if (!dto.Id.HasValue) return NotFound();

        try
        {
            // Get existing doctor to preserve photo if no new file uploaded
            var existing = await _doctorService.GetByIdAsync(dto.Id.Value);
            dto.PhotoUrl = await UploadPhotoAsync(dto.PhotoFile, existing?.PhotoUrl);

            var success = await _doctorService.UpdateAsync(dto);
            return success ? RedirectToAction(nameof(Index)) : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);  // ✅ Returns Edit.cshtml with errors
        }
    }

    // 🗑️ Soft delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _doctorService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}