namespace HospitalManagementSystem.ViewModels
{
    public class DoctorEditViewModel
    {
       
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Specialization { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
            public DateTime?StartDate { get; set; }
            public DateTime?EndDate { get; set; }
            public TimeSpan?StartTime { get; set; }
            public TimeSpan?EndTime { get; set; }
    }
}
