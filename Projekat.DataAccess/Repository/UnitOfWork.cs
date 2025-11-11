using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;

namespace VinylVibe.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IGenreRepository Genre { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IProductRepository Product { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IUserRepository User { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            ShoppingCart = new ShoppingCartRepository(_db);
            User = new UserRepository(_db);
            Genre = new GenreRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetails = new OrderDetailsRepository(_db); 
           
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
