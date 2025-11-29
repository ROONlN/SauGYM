using Microsoft.AspNetCore.Identity;

namespace SauGYM.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Height { get; set; } // Boy (cm)
        public double? Weight { get; set; } // Kilo (kg)
        public string? Gender { get; set; } // Cinsiyet
    }
}
