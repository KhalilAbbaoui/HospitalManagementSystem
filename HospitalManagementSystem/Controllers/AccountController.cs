using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers.Account
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Action to display the login page (accessible to everyone)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // Action to handle login (accessible to everyone)
        [AllowAnonymous]
        [HttpPost("Account/Login")]
        public async Task<IActionResult> Login(LoginVM model,string returnUrl=null)
        {
            ViewData["returnUrl"] = returnUrl;
            if (ModelState.IsValid)
                {
                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email is required.");
                    return View(model);
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {

                    HttpContext.Session.SetString("UserId", user.Id);

                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Patient"))
                    {
                        return RedirectToAction("Index", "Patients");
                    }
                    else if (roles.Contains("Doctor"))
                    {
                        return RedirectToAction("ViewPatient", "Doctors");
                    }
                    else if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        // Handle users with no role assignment (optional)
                        ModelState.AddModelError(string.Empty, "User has no assigned role.");
                        return View(model);
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }
      
        // Action to display the registration page (accessible to everyone)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Action to handle registration (accessible to everyone)
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber=model.PhoneNumber,
                    LockoutEnabled = false
                    
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Set TempData to trigger SweetAlert
                    TempData["RegistrationSuccess"] = "Registration successful. Please log in.";
                    return RedirectToAction("Register"); // Rediriger vers la même page pour afficher le SweetAlert
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Action to handle logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
