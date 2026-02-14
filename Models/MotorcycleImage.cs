using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class MotorcycleImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public int MotorcycleId { get; set; }

        [ForeignKey("MotorcycleId")]
        public Motorcycle Motorcycle { get; set; }
    }
}