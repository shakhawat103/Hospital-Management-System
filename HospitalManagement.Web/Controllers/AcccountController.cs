using HospitalManagement.Core.Entities;
using HospitalManagement.Core.Entities.Identity;
using HospitalManagement.Core.Enums;
using HospitalManagement.Infrastructure;
using HospitalManagement.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HospitalManagement.Web.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _context;

    public AccountController(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      ILogger<AccountController> logger,
      ApplicationDbContext context)  // ✅ Add this
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _context = context;
    }

    // 🔐 GET: /Account/Login
    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Index", "Home");
        return View();
    }

    // 🔐 POST: /Account/Login
    [HttpPost("Login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Index", "Home");
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return LocalRedirect(returnUrl ?? Url.Action("Index", "Home"));
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    // 📝 GET: /Account/Register
    [HttpGet("Register")]
    public IActionResult Register() => View();

    // 📝 POST: /Account/Register
   // UPDATE your Register POST action:
[HttpPost("Register"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // 1️⃣ Create Identity User
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // 2️⃣ Force Patient Role (no role selection on public form)
            await _userManager.AddToRoleAsync(user, UserRole.Patient);

            // 3️⃣ ✅ AUTO-CREATE PATIENT RECORD LINKED TO USER
            var nameParts = model.FullName.Trim().Split(' ', 2);
            var patient = new Patient
            {
                UserId = user.Id,  // 🔗 Critical: Links Patient ↔ Identity User
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                DateOfBirth = DateTime.UtcNow.AddYears(-20), // Default, patient can edit later
                Phone = "",
                Email = user.Email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();  // Saves Patient to DB

            // 4️⃣ Sign in immediately
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("New patient registered: {Email}", model.Email);

            // Redirect to patient dashboard
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // 🚪 POST: /Account/Logout
    [HttpPost("Logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    // 🚫 GET: /Account/AccessDenied
    [HttpGet("AccessDenied")]
    public IActionResult AccessDenied() => View();
}