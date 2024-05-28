using JobManagement.DotNet.Modals.Entities;

namespace JobManagement.DotNet.Models
{
    public class AddjobDto
    {
       
        public string Title { get; set; }
        public string Description { get; set; }
        public int LocationId { get; set; } // Assuming LocationId is the ID of the location
        public int DepartmentId { get; set; } // Only include DepartmentId instead of Department
        public DateTime ClosingDate { get; set; }
    }
}
