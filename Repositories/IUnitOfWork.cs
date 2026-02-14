using Store.Models;

namespace Store.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<ApplicationUser> ApplicationUser { get; }
        IRepository<Showroom> Showroom { get; }
        IRepository<Motorcycle> Motorcycle { get; }
        IRepository<InquiryHeader> InquiryHeader { get; }
        IRepository<InquiryDetail> InquiryDetail { get; }
        IRepository<ShoppingCart> ShoppingCart { get; }
        IRepository<MotorcycleImage> MotorcycleImage { get; }
        void Save();
    }
}