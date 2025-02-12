using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Hangfire;
using HospitalManagementSystem.ViewModels;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers.Appointments;

public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailNotificationService _emailService;
    private readonly SmsNotificationService _smsService;

    public AppointmentsController(ApplicationDbContext context, EmailNotificationService emailService, SmsNotificationService smsService)
    {
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
    }

    [Authorize(Roles = "Patient")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");
        if (role == null)
        {
            return NotFound("Role not found.");
        }

        var doctors = await _context.ApplicationUsers
            .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == role.Id))
            .ToListAsync();

        ViewBag.Doctors = new SelectList(doctors, "Id", "FullName");

        // Initialiser le modèle avec l'ID du patient
        var model = new AppointmentVM
        {
            PatientId = User.FindFirstValue(ClaimTypes.NameIdentifier) // Utiliser l'ID de l'utilisateur connecté
        };

        return View(model);
    }

    private async Task PopulateDoctorsSelectList(object selectedDoctor = null)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");
        var doctors = await _context.ApplicationUsers
            .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == role.Id))
            .ToListAsync();
        ViewBag.Doctors = new SelectList(doctors, "Id", "FullName", selectedDoctor);
    }

    [Authorize(Roles = "Patient")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentVM model)
    {
        if (ModelState.IsValid)
        {
            var appointment = new Appointment
            {
                Date = model.Date,
                Time = model.Time,
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                Service = model.Service
            };

            // Vérifier la disponibilité et la limite quotidienne
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.ApplicationUserId == appointment.DoctorId &&
                                          a.StartDate <= appointment.Date &&
                                          a.EndDate >= appointment.Date &&
                                          a.StartTime <= appointment.Time &&
                                          a.EndTime >= appointment.Time);

            if (availability == null)
            {
                ModelState.AddModelError("", "The selected time slot is not available.");
                await PopulateDoctorsSelectList(appointment.DoctorId);
                return View(model);
            }

            var dailyAppointmentCount = await _context.Appointments
                .CountAsync(a => a.DoctorId == appointment.DoctorId && a.Date == appointment.Date);

            var doctor = await _context.ApplicationUsers.FindAsync(appointment.DoctorId);
            if (doctor != null && dailyAppointmentCount >= doctor.DailyLimit)
            {
                ModelState.AddModelError("", "The doctor has reached the maximum number of appointments for today.");
                await PopulateDoctorsSelectList(appointment.DoctorId);
                return View(model);
            }

            _context.Add(appointment);
            await _context.SaveChangesAsync();

            // Ne pas envoyer les notifications ici
            TempData["SuccessMessage"] = "Appointment created successfully!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDoctorsSelectList(model.DoctorId);
        return View(model);
    }

    private void ScheduleAppointmentReminders(string patientId, DateTime appointmentTime)
    {
        var oneDayBefore = appointmentTime.AddDays(-1);
        var twoHoursBefore = appointmentTime.AddHours(-2);

        BackgroundJob.Schedule(() => SendAppointmentReminder(patientId, appointmentTime), oneDayBefore);
        BackgroundJob.Schedule(() => SendAppointmentReminder(patientId, appointmentTime), twoHoursBefore);
    }

    // Envoyer les rappels (e-mail et SMS)
    public async Task SendAppointmentReminder(string patientId, DateTime appointmentTime)
    {
        try
        {
            var patient = await _context.ApplicationUsers.FindAsync(patientId);
            if (patient != null)
            {
                await _emailService.SendAppointmentReminder(patient.Email, appointmentTime);
                await _smsService.SendSmsReminder(patient.PhoneNumber, appointmentTime);
            }
        }
        catch (Exception ex)
        {
            // Log l'erreur ou relancez-la pour que Hangfire puisse la gérer
            throw new Exception("Failed to send reminders", ex);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Patient")]
    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Récupérer l'ID de l'utilisateur connecté
        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Rediriger vers la page de connexion si l'utilisateur n'est pas connecté
        }

        // Récupérer les rendez-vous du patient connecté
        var appointments = _context.Appointments
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Where(p => p.PatientId == userId)
            .ToList();

        return View(appointments);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageAppointment()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ToListAsync();

        return View(appointments);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            appointment.IsConfirmed = true;
            await _context.SaveChangesAsync();

            // Envoyer les notifications uniquement si le rendez-vous est confirmé
            ScheduleAppointmentReminders(appointment.PatientId, appointment.Date.Add(appointment.Time));

            TempData["SuccessMessage"] = "Appointment confirmed successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Appointment not found.";
        }

        return RedirectToAction("Index", "Admin");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment canceled successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Appointment not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        var viewModel = new Appointment
        {
            Id = appointment.Id,
            Date = appointment.Date,
            Time = appointment.Time,
            DoctorId = appointment.DoctorId,
            Service = appointment.Service,
            PatientId = appointment.PatientId
        };

        await PopulateDoctorsSelectList(appointment.DoctorId);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Date = viewModel.Date;
            appointment.Time = viewModel.Time;
            appointment.DoctorId = viewModel.DoctorId;
            appointment.Service = viewModel.Service;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDoctorsSelectList(viewModel.DoctorId);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            TempData["ErrorMessage"] = "Appointment not found.";
            return RedirectToAction(nameof(Index));
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        TempData["DeleteMessage"] = "Appointment deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}