using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;

namespace Tests
{
    public class OrderServiceUnitTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IOrderHeaderRepository> _mockOrderHeaderRepo;
        private Mock<IOrderDetailsRepository> _mockOrderDetailsRepo;

        private OrderService _orderService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderHeaderRepo = new Mock<IOrderHeaderRepository>();
            _mockOrderDetailsRepo = new Mock<IOrderDetailsRepository>();

            _mockUnitOfWork.Setup(u => u.OrderHeader).Returns(_mockOrderHeaderRepo.Object);
            _mockUnitOfWork.Setup(u => u.OrderDetails).Returns(_mockOrderDetailsRepo.Object);

            var orderHeader = new OrderHeader { Id = 1, UserId = "user1", OrderTotal = 300, PaymentStatus = VinylVibe.Utility.Details.PaymentStatusApproved };
            var orderDetails = new List<OrderDetails>
            {
                new OrderDetails { Id = 1, OrderHeaderId = 1, Product = new Product { Title = "Thriller" }, Count = 2 },
                new OrderDetails { Id = 2, OrderHeaderId = 1, Product = new Product { Title = "Kind of Blue" }, Count = 1 }
        };

            _mockOrderHeaderRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()
            )).Returns((Expression<Func<OrderHeader, bool>> filter, string includeProperties, bool tracked) =>
            {
                return orderHeader;
            });

            _mockOrderDetailsRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OrderDetails, bool>>>(), It.IsAny<string>()))
                                 .Returns(orderDetails.AsQueryable());

            _orderService = new OrderService(_mockUnitOfWork.Object);
        }

        #region GetOrderDetailsTests
        [Test]
        public void GetOrderDetails_ExistingOrder_ReturnsOrderViewModel()
        {
            var result = _orderService.GetOrderDetails(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderHeader, Is.Not.Null);
            Assert.That(result.OrderHeader.Id, Is.EqualTo(1));
            Assert.That(result.OrderDetails.Count, Is.EqualTo(2));
            Assert.That(result.OrderDetails.ElementAt(0).Product.Title, Is.EqualTo("Thriller"));
            Assert.That(result.OrderDetails.ElementAt(1).Product.Title, Is.EqualTo("Kind of Blue"));
        }
        [Test]
        public void GetOrderDetails_OrderDoesNotExist_ReturnsNullOrderHeader()
        {
            _mockOrderHeaderRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()
            )).Returns((Expression<Func<OrderHeader, bool>> filter, string includeProperties, bool tracked) => null);

            var result = _orderService.GetOrderDetails(99);

            Assert.That(result.OrderHeader, Is.Null);

        }
        #endregion

        #region UpdateOrderTest
        [Test]
        public void UpdateOrder_AllFields_UpdatesOrder()
        {
            var existingOrder = new OrderHeader
            {
                Id = 1,
                Name = "Old Name",
                PhoneNumber = "123",
                City = "Old City",
                Country = "Old Country",
                StreetAddress = "Old Street",
                PostalCode = "0000",
                Carrier = "Old Carrier",
                TrackingNumber = "OldTrack"
            };

            _mockOrderHeaderRepo.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), null, false))
                                .Returns(existingOrder);

            var updatedOrder = new OrderViewModel
            {
                OrderHeader = new OrderHeader
                {
                    Id = 1,
                    Name = "New Name",
                    PhoneNumber = "456",
                    City = "New City",
                    Country = "New Country",
                    StreetAddress = "New Street",
                    PostalCode = "1111",
                    Carrier = "New Carrier",
                    TrackingNumber = "NewTrack"
                }
            };

            _orderService.UpdateOrder(updatedOrder);

            _mockOrderHeaderRepo.Verify(r => r.Update(It.Is<OrderHeader>(
                o => o.Name == "New Name" &&
                     o.PhoneNumber == "456" &&
                     o.City == "New City" &&
                     o.Country == "New Country" &&
                     o.StreetAddress == "New Street" &&
                     o.PostalCode == "1111" &&
                     o.Carrier == "New Carrier" &&
                     o.TrackingNumber == "NewTrack"
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        #endregion

        #region ChangingStatusTests
        [Test]
        public void StartProcessingOrder_UpdatesStatusAndSaves()
        {
            int orderId = 1;

            _orderService.StartProcessingOrder(orderId);

            _mockOrderHeaderRepo.Verify(r => r.UpdateStatus(
                orderId,
                VinylVibe.Utility.Details.StatusInProcess,
                null), Times.Once);

            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        [Test]
        public void ShipOrder_UpdatesOrderHeaderAndSaves()
        {
            int orderId = 1;
            string trackingNumber = "101010101";
            string carrier = "FON";

            _orderService.ShipOrder(orderId, trackingNumber, carrier);

            _mockOrderHeaderRepo.Verify(r => r.Update(It.Is<OrderHeader>(
                o => o.Id == orderId &&
                     o.TrackingNumber == trackingNumber &&
                     o.Carrier == carrier &&
                     o.OrderStatus == VinylVibe.Utility.Details.StatusShipped &&
                     o.ShippingDate.Date == DateTime.Now.Date
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void CancelOrder_PaymentApproved_UpdatesStatusToCancelledAndRefunded()
        {
            int orderId = 1;

            _orderService.CancelOrder(orderId);

            _mockOrderHeaderRepo.Verify(r => r.UpdateStatus(orderId,
                VinylVibe.Utility.Details.StatusCancelled,
                It.Is<string>(s => s == VinylVibe.Utility.Details.StatusRefunded)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        [Test]
        public void CancelOrder_PaymentNotApproved_UpdatesStatusToCancelled()
        {
            int orderId = 2;
            var orderHeader = new OrderHeader
            {
                Id = orderId,
                PaymentStatus = "DelayedPayment"
            };

            _mockOrderHeaderRepo.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
                                .Returns(orderHeader);

            _orderService.CancelOrder(orderId);

            _mockOrderHeaderRepo.Verify(r => r.UpdateStatus(orderId,
                VinylVibe.Utility.Details.StatusCancelled,
                VinylVibe.Utility.Details.StatusCancelled), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        [Test]
        public void CompletePayment_UpdatesPaymentStatusAndSaves()
        {
            int orderId = 1;
            var orderHeader = new OrderHeader { Id = orderId, PaymentStatus = "Pending", PaymentDate = DateTime.MinValue };

            _mockOrderHeaderRepo.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
                                .Returns(orderHeader);

            _orderService.CompletePayment(orderId);

            Assert.That(orderHeader.PaymentStatus, Is.EqualTo(VinylVibe.Utility.Details.PaymentStatusApproved));
            Assert.That(orderHeader.PaymentDate, Is.Not.Null);
            _mockOrderHeaderRepo.Verify(r => r.Update(orderHeader), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        #endregion

        #region GetOrderByStatusTests
        [Test]
        public void GetOrdersByStatus_AdminReturnsAllStatuses()
        {
            var orders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, UserId = "user1", OrderStatus = VinylVibe.Utility.Details.StatusInProcess },
                new OrderHeader { Id = 2, UserId = "user2", OrderStatus = VinylVibe.Utility.Details.StatusShipped },
                new OrderHeader { Id = 3, UserId = "user1", OrderStatus = VinylVibe.Utility.Details.StatusApproved },
            }.AsQueryable();

            _mockOrderHeaderRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
                                .Returns(orders);

            var result = _orderService.GetOrdersByStatus("inprocess", "user1", isAdmin: true).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].OrderStatus, Is.EqualTo(VinylVibe.Utility.Details.StatusInProcess));
        }
        [Test]
        public void GetOrdersByStatus_NonAdminFiltersByUser()
        {
            var orders = new List<OrderHeader>
            {
                 new OrderHeader { Id = 1, UserId = "user1", OrderStatus = VinylVibe.Utility.Details.StatusInProcess },
                 new OrderHeader { Id = 2, UserId = "user2", OrderStatus = VinylVibe.Utility.Details.StatusShipped },
                 new OrderHeader { Id = 3, UserId = "user1", OrderStatus = VinylVibe.Utility.Details.StatusApproved },
            }.AsQueryable();

            _mockOrderHeaderRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
                                .Returns(orders);

            var result = _orderService.GetOrdersByStatus("approved", "user1", isAdmin: false).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].UserId, Is.EqualTo("user1"));
            Assert.That(result[0].OrderStatus, Is.EqualTo(VinylVibe.Utility.Details.StatusApproved));
        }
        #endregion


    }
}
