using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Store.ViewModels
{
    public class MotorcycleFormVM
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Model Name")]
        public string Model { get; set; } 

        [Required, MaxLength(50)]
        public string Brand { get; set; }

        [Required]
        [Range(1, 10000000)]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Motorcycle Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Quantity in Stock")]
        public int StockQuantity { get; set; }


        [Display(Name = "Select Showrooms")]
        [Required(ErrorMessage = "Please select at least one showroom")]
        public List<int> ShowroomIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? Showrooms { get; set; }
    }
}