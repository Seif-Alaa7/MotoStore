using System.ComponentModel.DataAnnotations;

namespace Store.ViewModels
{
    public class ShowroomFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the showroom name.")]
        [Display(Name = "Showroom Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Display(Name = "Address")]
        [MaxLength(255)]
        public string Address { get; set; }

        [Display(Name = "Showroom Logo")]
        public IFormFile? LogoFile { get; set; }
    }
}