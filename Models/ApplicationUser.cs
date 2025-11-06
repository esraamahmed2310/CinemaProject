using Microsoft.AspNetCore.Identity;

namespace CinemaProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;

    }
}
