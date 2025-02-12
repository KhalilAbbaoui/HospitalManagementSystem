using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models
{
    public class Treatment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; }

        [NotMapped]
        public ApplicationUser Patient { get; set; }

        public string Description { get; set; }

        [ForeignKey(nameof(Patient))]
        public string DoctortId { get; set; }
        [NotMapped]
        public ApplicationUser Doctor { get; set; }
    }
}
