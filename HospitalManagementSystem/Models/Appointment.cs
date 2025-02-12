using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace HospitalManagementSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        public string Service { get; set; }

        public bool IsConfirmed { get; set; }
   

        [ForeignKey(nameof(Patient))]
        public string PatientId { get; set; }

        [ForeignKey(nameof(Doctor))]
        public string DoctorId { get; set; }

        // Navigation properties
        
        public ApplicationUser? Patient { get; set; }
        
        public ApplicationUser? Doctor { get; set; }
    }
}
