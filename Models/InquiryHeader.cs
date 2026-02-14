using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class InquiryHeader
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public int ShowroomId { get; set; }
        [ForeignKey("ShowroomId")]
        [ValidateNever]
        public Showroom Showroom { get; set; }

        public DateTime InquiryDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please enter your full name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be at least 3 characters")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z\s]+$", ErrorMessage = "Name must contain letters only (No numbers or symbols)")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Invalid Egyptian phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }
}