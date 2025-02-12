using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers.Doctors
{
    [Authorize(Roles = "Doctor")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Doctors/ViewPatient/5
        [HttpGet]
        public async Task<IActionResult> ViewPatient(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Users
                .Include(p => p.Treatments) // Ensure Treatments is properly configured in ApplicationUser
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            var viewModel = new PatientWithTreatmentVM
            {
                Patient = patient,
                PatientId = patient.Id,
                TreatmentDescription = string.Empty
            };

            return View(viewModel);
        }


        // POST: Doctors/AddTreatment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTreatment(string patientId, [Bind("Description")] Treatment treatment)
        {
            // Authorization check: Ensure the user is a doctor
            if (!User.IsInRole("Doctor"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                treatment.PatientId = patientId;
                _context.Treatments.Add(treatment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Treatment description added successfully.";
                return RedirectToAction(nameof(ViewPatient), new { id = patientId });
            }

            TempData["ErrorMessage"] = "Failed to add treatment description.";
            return RedirectToAction(nameof(ViewPatient), new { id = patientId });
        }
    }
}
