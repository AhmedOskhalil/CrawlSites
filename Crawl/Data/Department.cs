namespace Crawl.Data
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // No CountryId or Country property
        public List<Employee> Employees { get; set; } = new();
    }
}
