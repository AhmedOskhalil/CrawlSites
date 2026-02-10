using Crawl.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class EmployeeAuthService
{
    private readonly AppDbContext _db;

    public EmployeeAuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Employee?> LoginAsync(string email, string password)
    {
        // For production, use hashed passwords!
        return await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Email == email && e.Password == password);
    }
}