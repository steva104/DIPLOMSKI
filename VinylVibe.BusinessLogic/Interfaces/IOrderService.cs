using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.Models.ViewModel;
using VinylVibe.Models;

namespace VinylVibe.BusinessLogic.Interfaces
{
	public interface IOrderService
	{
		OrderViewModel GetOrderDetails(int orderId);
		void UpdateOrder(OrderViewModel orderVM);
		void StartProcessingOrder(int orderId);
		void ShipOrder(int orderId, string trackingNumber, string carrier);
		void CancelOrder(int orderId);
		void CompletePayment(int orderId);
		IQueryable<OrderHeader> GetOrdersByStatus(string status, string userId, bool isAdmin);
		IQueryable<OrderHeader> ApplySorting(IQueryable<OrderHeader> query, string sortColumn, string sortDirection);


    }
}
