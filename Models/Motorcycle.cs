using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class Motorcycle
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Model { get; set; }

        [Required]
        public string Brand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public int ShowroomId { get; set; }

        [ForeignKey("ShowroomId")]
        public virtual Showroom Showroom { get; set; }
    }
}