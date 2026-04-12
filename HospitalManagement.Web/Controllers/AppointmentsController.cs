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
    public async Task<IActionResult> Book(int doctorId)
    {
        // Pre-fill the form with selected doctor
        var dto = new CreateAppointmentDto { DoctorId = doctorId };

        // Get current patient's ID (from logged-in user)
        var currentUser = await _patientService.GetCurrentPatientIdAsync(User);
        if (currentUser.HasValue)
            dto.PatientId = currentUser.Value;

        return View(dto);
    }

    // === 📅 PATIENT: Handle Booking Submission ===
    [Authorize(Roles = "Patient")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        // Ensure patient is booking for themselves (security)
        var currentPatientId = await _patientService.GetCurrentPatientIdAsync(User);
        if (!currentPatientId.HasValue || currentPatientId.Value != dto.PatientId)
        {
            ModelState.AddModelError(string.Empty, "You can only book appointments for yourself");
            return View(dto);
        }

        try
        {
            var booked = await _appointmentService.BookAppointmentAsync(dto);
            return RedirectToAction("MyAppointments");
        }
        catch (InvalidOperationException ex)
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