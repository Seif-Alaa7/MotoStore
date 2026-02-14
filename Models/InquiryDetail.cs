using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class InquiryDetail
    {
        public int Id { get; set; }

        [Required]
        public int InquiryHeaderId { get; set; }
        [ForeignKey("InquiryHeaderId")]
        [ValidateNever]
        public InquiryHeader InquiryHeader { get; set; }

        [Required]
        public int MotorcycleId { get; set; }
        [ForeignKey("MotorcycleId")]
        [ValidateNever]
        public Motorcycle Motorcycle { get; set; }

        public int Count { get; set; } 
    }
}