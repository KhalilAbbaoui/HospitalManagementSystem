using HospitalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.ViewModels
{
    public class PatientWithTreatmentVM
    {
        public string PatientId { get; set; }

        public ApplicationUser Patient { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string TreatmentDescription { get; set; }

    }

}
