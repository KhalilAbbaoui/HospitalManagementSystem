using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.ViewModels
{
    public class ManageAviability
    {
        public string FullName { get; set; }
        public string  Email { get; set; }
        public string Specialization { get; set; }
        public string? Address { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
