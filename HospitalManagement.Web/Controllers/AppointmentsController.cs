using HospitalManagement.Application.Services.AppointmentService;
using HospitalManagement.Application.Services.PatientService;

using HospitalManagement.Core.DTOs.Appointments;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers;

[Authorize]  // ✅ All actions require login
public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;  // To get current patient's ID

    public AppointmentsController(
        IAppointmentService appointmentService,
        IPatientService patientService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
    }

    // === 👨‍⚕️ PATIENT: Browse Available Doctors ===
    [Authorize(Roles = "Patient")]  // ✅ Only patients can browse doctors to book
    public async Task<IActionResult> BrowseDoctors()
    {
        var doctors = await _appointmentService.GetAvailableDoctorsAsync();
        return View(doctors);
    }

    // === 📅 PATIENT: Book Appointment Form ===
    [Authorize(Roles = "Patient")]
    [HttpGet]  // ✅ REMOVE the "Book/{doctorId?}" string - use default routing
    public async Task<IActionResult> Book(int? doctorId)
    {
        var patientId = await _patientService.GetCurrentPatientIdAsync(User);
        if (!patientId.HasValue)
            return RedirectToAction("Index", "Home");

        var dto = new CreateAppointmentDto
        {
            PatientId = patientId.Value,
            DoctorId = doctorId ?? 0,
            AppointmentDate = DateTime.Now.AddHours(2)
        };

        return View(dto);
    }


    // === 📅 PATIENT: Handle Booking Submission ===
    [Authorize(Roles = "Patient")]
    [HttpPost]  // ✅ Keep this - matches default convention
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(CreateAppointmentDto dto)
    {
        var currentPatientId = await _patientService.GetCurrentPatientIdAsync(User);
        if (!currentPatientId.HasValue || currentPatientId.Value != dto.PatientId)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _appointmentService.BookAppointmentAsync(dto);
            TempData["Success"] = "✅ Appointment booked successfully!";
            return RedirectToAction("MyAppointments");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    // === 📋 PATIENT: View My Appointments ===
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> MyAppointments()
    {
        // Get current patient's ID
        var patientId = await _patientService.GetCurrentPatientIdAsync(User);
        if (!patientId.HasValue)
            return RedirectToAction("Index", "Home");  // Or show error

        var appointments = await _appointmentService.GetPatientAppointmentsAsync(patientId.Value);
        return View(appointments);
    }

    // === ❌ PATIENT: Cancel Appointment ===
    [Authorize(Roles = "Patient")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason = "Cancelled by patient")
    {
        // Verify ownership: patient can only cancel their own appointments
        var patientId = await _patientService.GetCurrentPatientIdAsync(User);
        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment == null || appointment.PatientId != patientId)
            return NotFound();

        var result = await _appointmentService.CancelAppointmentAsync(id, reason);
        return result ? RedirectToAction("MyAppointments") : BadRequest("Could not cancel appointment");
    }

    // === 👨‍⚕️ DOCTOR: View Their Appointments (Optional) ===
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> DoctorSchedule()
    {
        // Implementation for doctors to see their schedule
        // (Similar to patient view but filtered by doctor ID)
        // For now, return placeholder
        return View();
    }
}