using HospitalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.ViewModels
{
    public class PatientVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // Navigation property for related Appointments
        public IList<Appointment> Appointments { get; set; }

        // Navigation property for related Treatments
        public IList<Treatment> Treatments { get; set; }

  
    }
}
