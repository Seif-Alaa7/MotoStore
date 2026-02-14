using Store.Data;
using Store.Models;

namespace Store.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IRepository<ApplicationUser> ApplicationUser { get; private set; }
        public IRepository<Showroom> Showroom { get; private set; }
        public IRepository<Motorcycle> Motorcycle { get; private set; }
        public IRepository<InquiryHeader> InquiryHeader { get; private set; }
        public IRepository<InquiryDetail> InquiryDetail { get; private set; }
        public IRepository<ShoppingCart> ShoppingCart { get; private set; }
        public IRepository<MotorcycleImage> MotorcycleImage { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Showroom = new Repository<Showroom>(_db);
            Motorcycle = new Repository<Motorcycle>(_db);
            InquiryHeader = new Repository<InquiryHeader>(_db);
            InquiryDetail = new Repository<InquiryDetail>(_db);
            ShoppingCart = new Repository<ShoppingCart>(_db);
            MotorcycleImage = new Repository<MotorcycleImage>(_db);
            ApplicationUser = new Repository<ApplicationUser>(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}