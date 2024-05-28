namespace JobManagement.DotNet.Models
{
    public class JobListRequest
    {
        public string Q { get; set; } // Search string
        public int PageNo { get; set; } = 1; // Page number
        public int PageSize { get; set; } = 10; // Page size
        public int? LocationId { get; set; } // Optional location id
        public int? DepartmentId { get; set; } // Optional department id
    }
}
