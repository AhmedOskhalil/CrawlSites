namespace Crawl.Data
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string Password { get; set; } // Add password
        public string Role { get; set; }     // "Manager" or "Employee"

        public int Salary { get; set; }
        public int? Age { get; set; }
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }
    }

}
