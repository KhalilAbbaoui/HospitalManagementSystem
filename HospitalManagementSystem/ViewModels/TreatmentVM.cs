using HospitalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.ViewModels
{
    public class TreatmentVM
    {

        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        // Navigation property
        public ApplicationUser Patient { get; set; }
    }
}
