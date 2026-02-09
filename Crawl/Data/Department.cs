namespace Crawl.Data
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }
        public List<Employee> Employees { get; set; }


    }
}
