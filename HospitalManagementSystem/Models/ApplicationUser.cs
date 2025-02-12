using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models
{
    public class ApplicationUser:IdentityUser
    {

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [NotMapped]
        public IList<Appointment>? Appointments  { get; set; }
        [NotMapped]
        public IList<Treatment>?Treatments { get; set; }
        public int? DailyLimit { get; set; }
        public string? Specialization {  get; set; }

    }
}
