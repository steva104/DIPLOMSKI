using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IGenreRepository Genre{ get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IUserRepository User { get; }

        IOrderDetailsRepository OrderDetails { get; }

        IOrderHeaderRepository OrderHeader { get; }
        void Save();

    }
}
