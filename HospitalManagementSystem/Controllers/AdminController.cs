using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public async Task<IActionResult> Index(string searchString)
        {
            // Récupérer les statistiques
            var patientRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Patient");
            var totalPatients = await _context.ApplicationUsers
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == patientRole.Id))
                .CountAsync();

            var totalAppointments = await _context.Appointments.CountAsync();

            ViewBag.TotalPatients = totalPatients;
            ViewBag.TotalAppointments = totalAppointments;

            // Filtrer les médecins par nom si un terme de recherche est fourni
            var doctorsQuery = from user in _context.ApplicationUsers
                               join userRole in _context.UserRoles on user.Id equals userRole.UserId
                               join role in _context.Roles on userRole.RoleId equals role.Id
                               join availability in _context.Availabilities on user.Id equals availability.ApplicationUserId into availabilities
                               from availability in availabilities.DefaultIfEmpty()
                               where role.Name == "Doctor"
                               orderby user.FullName
                               select new
                               {
                                   user.Id,
                                   user.FullName,
                                   user.Specialization,
                                   user.Email,
                                   user.Address,
                                   StartDate = availability != null ? (DateOnly?)DateOnly.FromDateTime(availability.StartDate.Date) : null,
                                   EndDate = availability != null ? (DateOnly?)DateOnly.FromDateTime(availability.EndDate.Date) : null,
                                   StartTime = availability != null ? (TimeSpan?)availability.StartTime : null,
                                   EndTime = availability != null ? (TimeSpan?)availability.EndTime : null
                               };

            if (!string.IsNullOrEmpty(searchString))
            {
                doctorsQuery = doctorsQuery.Where(d => d.FullName.Contains(searchString));
            }

            var usersWithRole = await doctorsQuery.ToListAsync();

            return View(usersWithRole);
        }

        // GET: Admin/CreateDoctor
        [HttpGet]
        public IActionResult CreateDoctor()
        {
            var model = new ManageAviability
            {
                StartTime = TimeSpan.FromHours(9), // Default start time
                EndTime = TimeSpan.FromHours(17)  // Default end time
            };
            return View(model);
        }

        // POST: Admin/CreateDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor([Bind("FullName,Specialization,Email,Address,StartDate,EndDate,StartTime,EndTime")] ManageAviability doctorModel)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = doctorModel.Email,
                    Email = doctorModel.Email,
                    DateOfBirth = DateTime.Now,
                    FullName = doctorModel.FullName,
                    Specialization = doctorModel.Specialization,
                    Address=doctorModel.Address
                };

                var result = await _userManager.CreateAsync(user, "Doctor@123"); // Generate a secure password

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Doctor"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Doctor"));
                    }

                    await _userManager.AddToRoleAsync(user, "Doctor");

                    var availability = new Availability
                    {
                        ApplicationUserId = user.Id,
                        StartDate = doctorModel.StartDate.ToDateTime(new TimeOnly(0, 0)),
                        EndDate = doctorModel.EndDate.ToDateTime(new TimeOnly(0, 0)),
                        StartTime = doctorModel.StartTime,
                        EndTime = doctorModel.EndTime
                    };

                    await _context.Availabilities.AddAsync(availability);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Doctor added successfully!";
                    return RedirectToAction("CreateDoctor");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add the doctor. Please check the errors.";
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(doctorModel);
        }

        public async Task<IActionResult> PatientStatistics()
        {
            // Récupérer le rôle "Patient"
            var patientRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Patient");

            if (patientRole == null)
            {
                return NotFound("Role 'Patient' not found.");
            }

            // Compter le nombre total de patients
            var totalPatients = await _context.ApplicationUsers
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == patientRole.Id))
                .CountAsync();

            // Compter le nombre total de rendez-vous
            var totalAppointments = await _context.Appointments.CountAsync();


            // Créer le modèle de vue
            var viewModel = new PatientStatisticsVM
            {
                TotalPatients = totalPatients,
                TotalAppointments = totalAppointments,
               
            };

            return View(viewModel);
        }

        // GET: Admin/EditDoctor/5
        [HttpGet]
        public async Task<IActionResult> EditDoctor(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.ApplicationUsers
                .Where(u => u.Id == id)
                .Select(u => new DoctorEditViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Specialization = u.Specialization,
                    Email = u.Email,
                    Address = u.Address,
                    // Retrieve availability data
                    StartDate = _context.Availabilities.Where(a => a.ApplicationUserId == u.Id).Select(a => (DateTime?)a.StartDate).FirstOrDefault(),
                    EndDate = _context.Availabilities.Where(a => a.ApplicationUserId == u.Id).Select(a => (DateTime?)a.EndDate).FirstOrDefault(),
                    StartTime = _context.Availabilities.Where(a => a.ApplicationUserId == u.Id).Select(a => (TimeSpan?)a.StartTime).FirstOrDefault(),
                    EndTime = _context.Availabilities.Where(a => a.ApplicationUserId == u.Id).Select(a => (TimeSpan?)a.EndTime).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }



        // POST: Admin/EditDoctor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(string id, [Bind("Id,FullName,Specialization,Email,Address,StartDate,EndDate,StartTime,EndTime")] DoctorEditViewModel doctorModel)
        {
            if (id != doctorModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the doctor details
                    var doctor = await _context.ApplicationUsers.FindAsync(id);
                    if (doctor == null)
                    {
                        return NotFound();
                    }

                    doctor.FullName = doctorModel.FullName;
                    doctor.Specialization = doctorModel.Specialization;
                    doctor.Email = doctorModel.Email;
                    doctor.Address = doctorModel.Address;

                    _context.Update(doctor);

                    // Update or create availability details
                    var availability = await _context.Availabilities
                        .FirstOrDefaultAsync(a => a.ApplicationUserId == id);

                    if (availability == null)
                    {
                        // Create new availability if it doesn't exist
                        if (doctorModel.StartDate.HasValue && doctorModel.EndDate.HasValue && doctorModel.StartTime.HasValue && doctorModel.EndTime.HasValue)
                        {
                            availability = new Availability
                            {
                                ApplicationUserId = id,
                                StartDate = doctorModel.StartDate.Value,
                                EndDate = doctorModel.EndDate.Value,
                                StartTime = doctorModel.StartTime.Value,
                                EndTime = doctorModel.EndTime.Value
                            };
                            _context.Availabilities.Add(availability);
                        }
                    }
                    else
                    {
                        // Update existing availability
                        if (doctorModel.StartDate.HasValue)
                            availability.StartDate = doctorModel.StartDate.Value;
                        if (doctorModel.EndDate.HasValue)
                            availability.EndDate = doctorModel.EndDate.Value;
                        if (doctorModel.StartTime.HasValue)
                            availability.StartTime = doctorModel.StartTime.Value;
                        if (doctorModel.EndTime.HasValue)
                            availability.EndTime = doctorModel.EndTime.Value;

                        _context.Update(availability);
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Doctor updated successfully!";
                    return RedirectToAction("EditDoctor", new { id = doctor.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(doctorModel);
        }


        // GET: Admin/DeleteDoctor/5
        [HttpGet]
        public async Task<IActionResult> DeleteDoctor(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Admin/DeleteDoctor/5
        [HttpPost, ActionName("DeleteDoctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctorConfirmed(string id)
        {
            var doctor = await _context.Users.FindAsync(id);
            if (doctor != null)
            {
                _context.Users.Remove(doctor);
                await _context.SaveChangesAsync();

                // Delete associated IdentityUser
                var user = await _userManager.FindByEmailAsync(doctor.Email);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                TempData["SuccessMessage"] = "Doctor deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

     
        private bool DoctorExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

}