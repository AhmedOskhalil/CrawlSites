using Crawl.Data;

public class Employee
{
    public string UserId { get; set; }
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public int Salary { get; set; }
    public int? Age { get; set; }
    public int? CountryId { get; set; }
    public Country? Country { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; }
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public bool IsFirstLogin { get; set; } = true; // <-- NEW
}