using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Crawl.Data
{
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
        public int Age { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}