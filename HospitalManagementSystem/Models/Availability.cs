using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models
{
    public class Availability
    {
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }
        [NotMapped]
        public ApplicationUser? ApplicationUser { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
