using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VinylVibe.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {

        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(ShoppingCart cart)
        {
           _db.ShoppingCarts.Update(cart);
        }
        //za reset korpe
        public void RemoveRange(IEnumerable<ShoppingCart> cartItems)
        {
            _db.ShoppingCarts.RemoveRange(cartItems);
        }
    }
}
