using Store.Models;

namespace Store.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> CartList { get; set; }
        public decimal OrderTotal { get; set; }

        public InquiryHeader InquiryHeader { get; set; }
    }
}