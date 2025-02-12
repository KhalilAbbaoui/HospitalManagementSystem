using HospitalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.ViewModels
{
    public class DoctorVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Specialization")]
        public string Specialization { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Navigation property for related Appointments
        public ICollection<Appointment> Appointments { get; set; }

        [Required]
        [Display(Name = "Daily Appointment Limit")]
        public int DailyLimit { get; set; }
    }
}
