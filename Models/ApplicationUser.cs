using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }
        public virtual ICollection<Showroom> Showrooms { get; set; }
    }
}