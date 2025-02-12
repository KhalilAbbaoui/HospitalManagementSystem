using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;




namespace HospitalManagementSystem.Controllers.Patients
{
    [Authorize(Roles = "Patient")]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // GET: Patients/Edit/5
        
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the user from AspNetUsers table using UserManager
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            // Optionally, map the user data to a view model
            return View(user);
        }


        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FullName,Email,PhoneNumber,Address,DateOfBirth")] ApplicationUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update user data in AspNetUsers table
                    var existingUser = await _userManager.FindByIdAsync(id.ToString());
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Update only the properties that were edited
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.UserName = user.Email; // Update if Username is tied to Email
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.Address = user.Address;
                    existingUser.DateOfBirth = user.DateOfBirth;
                    existingUser.EmailConfirmed = true; // Optional: Confirm Email Automatically

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "informations updated successfully!";
                        return RedirectToAction("Edit");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error updating informations.";
                        return View(user);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
            }
            TempData["ErrorMessage"] = "Error updating user.";
            return View(user);
        }

        private bool PatientExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // GET: Patients/Delete/5
       
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the user from AspNetUsers table using UserManager
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // POST: Patients/Delete/5
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error deleting user.";
                    return View(user);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }
            return RedirectToAction(nameof(Index));
        }


    }
} 