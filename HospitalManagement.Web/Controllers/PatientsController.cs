using Microsoft.AspNetCore.Mvc;
using HospitalManagement.Core.DTOs.Patients;
using HospitalManagement.Application.Services.PatientService;
using Microsoft.AspNetCore.Authorization;

namespace HospitalManagement.Web.Controllers;

[Authorize(Roles ="Admin")] 
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService) =>
        _patientService = patientService;

    // 📋 GET: /Patients - List all
    public async Task<IActionResult> Index()
    {
        var patients = await _patientService.GetAllAsync();
        return View(patients);
    }

    // 👁️ GET: /Patients/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        return patient != null ? View(patient) : NotFound();
    }

    // ➕ GET: /Patients/Create - Show empty form
    public IActionResult Create() => View(new CreatePatientDto());

    // ➕ POST: /Patients/Create - Handle new patient
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

    // ✏️ GET: /Patients/Edit/5 - Show pre-filled form
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null) return NotFound();

        // Reuse CreatePatientDto for Edit: split FullName, add Id
        var names = patient.FullName.Split(' ', 2);
        var dto = new CreatePatientDto
        {
            Id = patient.Id,  // ✅ Set Id for Edit
            FirstName = names[0],
            LastName = names.Length > 1 ? names[1] : "",
            DateOfBirth = patient.DateOfBirth,
            Phone = patient.Phone,
            Email = patient.Email
        };
        return View("Create", dto);  // ✅ Reuse Create.cshtml view!
    }

    // ✏️ POST: /Patients/Edit - Handle update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CreatePatientDto dto)
    {
        if (!ModelState.IsValid) return View("Create", dto);  // Reuse Create view for errors
        if (!dto.Id.HasValue) return NotFound();

        try
        {
            var success = await _patientService.UpdateAsync(dto);
            return success ? RedirectToAction(nameof(Index)) : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Create", dto);  // Reuse Create view for errors
        }
    }

    // 🗑️ POST: /Patients/Delete/5 - Soft delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}