using HospitalManagement.Application.Services.PatientService;
using HospitalManagement.Core.DTOs.Patients;
using HospitalManagement.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize] 
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService, UserManager<ApplicationUser> userManager) // Add parameter
    {
        _patientService = patientService;
    }

    // 📋 GET: /Patients - List all
    // 📋 GET: /Patients?page=2&pageSize=10&search=John
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        // 1. Get paged + filtered patients from service
        var pagedResult = await _patientService.GetPagedAsync(page, pageSize, search);

        // 2. Pass metadata to view via ViewBag
        ViewBag.CurrentPage = pagedResult.PageNumber;
        ViewBag.PageSize = pagedResult.PageSize;
        ViewBag.TotalCount = pagedResult.TotalCount;
        ViewBag.SearchTerm = search;
        ViewBag.TotalPages = pagedResult.TotalPages;

        // 3. Return just the list of DTOs to the view
        return View(pagedResult);
    }

    // 👁️ GET: /Patients/Details/5
    [Authorize(Roles = "Admin,Patient")]
    public async Task<IActionResult> Details(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        return patient != null ? View(patient) : NotFound();
    }

    // ➕ GET: /Patients/Create - Show empty form
    public IActionResult Create() => View(new CreatePatientDto());

    // ➕ POST: /Patients/Create - Handle new patient
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _patientService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    // 🧑‍🦱 Quick link for patients to edit themselves
    [HttpGet("EditProfile")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> EditProfile()
    {
        var patientId = await _patientService.GetCurrentPatientIdAsync(User);
        if (!patientId.HasValue) return RedirectToAction("Index", "Home");

        return RedirectToAction("Edit", new { id = patientId.Value });
    }

    // ✅ GET: /Patients/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _patientService.GetForEditAsync(id);
        if (patient == null) return NotFound();

        // 🔒 Security: Patients can ONLY edit themselves
        if (User.IsInRole("Patient"))
        {
            var currentPatientId = await _patientService.GetCurrentPatientIdAsync(User);
            if (currentPatientId != id) return Forbid(); // 403 if trying to edit others
        }

        // Pass role context to view
        ViewBag.IsSelfEdit = User.IsInRole("Patient");
        return View(patient);
    }

    // ✅ POST: /Patients/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CreatePatientDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.IsSelfEdit = User.IsInRole("Patient");
            return View(dto);
        }

        // 🔒 Security check before saving
        if (User.IsInRole("Patient"))
        {
            var currentPatientId = await _patientService.GetCurrentPatientIdAsync(User);
            if (currentPatientId != dto.Id) return Forbid();
        }

        var success = await _patientService.UpdateAsync(dto);
        if (!success) return NotFound();

        // 🔄 Redirect based on WHO is editing
        if (User.IsInRole("Patient"))
            return RedirectToAction("Index", "Home"); // Patient → Home
        else
            return RedirectToAction("Index");         // Admin/Staff → Patients Index
    }


    // 🗑️ POST: /Patients/Delete/5 - Soft delete
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}