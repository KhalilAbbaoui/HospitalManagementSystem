using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AvailabilityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Availability/ManageAvailability
        public async Task<IActionResult> ManageAvailability()
        {
            var availabilities = await _context.Availabilities
                .Include(a => a.ApplicationUser)
                .ToListAsync();
            return View(availabilities);
        }

        [HttpGet]
        // GET: Availability/CreateAvailability
        public IActionResult CreateAvailability()
        {
            ViewData["Doctors"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: Availability/CreateAvailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAvailability([Bind("DoctorId,StartDate,EndDate,StartTime,EndTime")] Availability availability)
        {
            if (ModelState.IsValid)
            {
                _context.Add(availability);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Availability created successfully!";
                return RedirectToAction(nameof(ManageAvailability));
            }
            ViewData["Doctors"] = new SelectList(_context.Users, "Id", "FullName", availability.ApplicationUserId);
            return View(availability);
        }

        [HttpGet]
        // GET: Availability/EditAvailability/5
        public async Task<IActionResult> EditAvailability(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var availability = await _context.Availabilities.FindAsync(id);
            if (availability == null)
            {
                return NotFound();
            }
            ViewData["Doctors"] = new SelectList(_context.Users, "Id", "FullName", availability.ApplicationUserId);
            return View(availability);
        }

        // POST: Availability/EditAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAvailability(int id, [Bind("Id,DoctorId,StartDate,EndDate,StartTime,EndTime")] Availability availability)
        {
            if (id != availability.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(availability);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Availability updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AvailabilityExists(availability.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ManageAvailability));
            }
            ViewData["Doctors"] = new SelectList(_context.Users, "Id", "FullName", availability.ApplicationUserId);
            return View(availability);
        }

        // GET: Availability/DeleteAvailability/5
        [HttpGet]
        public async Task<IActionResult> DeleteAvailability(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var availability = await _context.Availabilities
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (availability == null)
            {
                return NotFound();
            }

            return View(availability);
        }

        // POST: Availability/DeleteAvailability/5
        [HttpPost, ActionName("DeleteAvailability")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailabilityConfirmed(int id)
        {
            var availability = await _context.Availabilities.FindAsync(id);
            if (availability != null)
            {
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Availability deleted successfully!";
            }
            return RedirectToAction(nameof(ManageAvailability));
        }

        private bool AvailabilityExists(int id)
        {
            return _context.Availabilities.Any(e => e.Id == id);
        }
    }
}
