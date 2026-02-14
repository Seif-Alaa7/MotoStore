using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class Showroom
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Address { get; set; }
        public string? LogoUrl { get; set; }
        public string VendorId { get; set; }

        [ForeignKey("VendorId")]
        public virtual ApplicationUser Vendor { get; set; }
        public virtual ICollection<Motorcycle> Motorcycles { get; set; }
    }
}