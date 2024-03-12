using Microsoft.AspNetCore.Identity;

namespace RESTaurantAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set;}
    }
}
