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
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {

        private ApplicationDbContext _db;
        public OrderDetailsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       

        public void Update(OrderDetails orderDetails)
        {
           _db.OrderDetails.Update(orderDetails);
        }
    }
}
