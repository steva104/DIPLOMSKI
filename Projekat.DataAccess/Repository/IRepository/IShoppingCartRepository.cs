using VinylVibe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart cart);
        //za reset korpe
        void RemoveRange(IEnumerable<ShoppingCart> cartItems);
    }

}

