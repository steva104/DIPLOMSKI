using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Data;
using VinylVibe.DataAccess.Repository;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;

namespace Tests.IntegrationTests
{
    public class OrderIntegrationTests
    {
        private ApplicationDbContext _dbContext;
        private IUnitOfWork _unitOfWork;
        private OrderService _orderService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            SeedData(_dbContext);

            _unitOfWork = new UnitOfWork(_dbContext);
            _orderService = new OrderService(_unitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }

        private void SeedData(ApplicationDbContext dbContext)
        {
            var user = new User { Id = "u1", UserName = "testuser", Name = "Test User" };
            var genre = new Genre { Id = 1, Name = "Rock" };
            var product = new Product
            {
                Id = 1,
                Title = "Guitar Album",
                Description = "A cool guitar album",
                Artist = "Rock Band",
                UPC = "123456",
                Year = 2023,
                ListPrice = 100,
                Price = 90,
                Price5 = 80,
                Price10 = 70,
                GenreId = 1,
                Genre = genre
            };

            var orderHeader = new OrderHeader
            {
                Id = 1,
                UserId = user.Id,
                User = user,
                OrderDate = DateTime.Now,
                ShippingDate = DateTime.Now.AddDays(2),
                OrderTotal = 90,
                OrderStatus = "Approved",
                PaymentStatus = "Approved",
                Name = "Test User",
                PhoneNumber = "123456789",
                StreetAddress = "Test Street 1",
                City = "TestCity",
                Country = "TestCountry",
                PostalCode = "12345",
                PaymentDate = DateTime.Now
            };

            var orderDetail = new OrderDetails
            {
                Id = 1,
                OrderHeaderId = orderHeader.Id,
                OrderHeader = orderHeader,
                ProductId = product.Id,
                Product = product,
                Count = 2,
                Price = 90
            };

            dbContext.Users.Add(user);
            dbContext.Genres.Add(genre);
            dbContext.Products.Add(product);
            dbContext.OrderHeaders.Add(orderHeader);
            dbContext.OrderDetails.Add(orderDetail);

            dbContext.SaveChanges();

            foreach (var entry in dbContext.ChangeTracker.Entries())
                entry.State = EntityState.Detached;
        }
        #region OrderDetailsIntegrationTests
        [Test]
        public void GetOrderDetails_WhenOrderExists_ReturnsOrderWithDetails()
        {
            var result = _orderService.GetOrderDetails(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderHeader, Is.Not.Null);
            Assert.That(result.OrderHeader.User, Is.Not.Null);
            Assert.That(result.OrderHeader.User.Name, Is.EqualTo("Test User"));

            Assert.That(result.OrderDetails.Count, Is.EqualTo(1));
            Assert.That(result.OrderDetails.First().Product.Title, Is.EqualTo("Guitar Album"));
            Assert.That(result.OrderDetails.First().Count, Is.EqualTo(2));
        }

        [Test]
        public void GetOrderDetails_WhenOrderDoesNotExist_ReturnsNullHeaderAndEmptyDetails()
        {
            var result = _orderService.GetOrderDetails(99);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.OrderHeader, Is.Null);
            Assert.That(result.OrderDetails, Is.Empty);
        }
        #endregion

        #region UpdateOrderIntegrationTest
        [Test]
        public void UpdateOrder_WhenOrderExists_UpdatesFields()
        {
            var updatedOrderVM = new OrderViewModel
            {
                OrderHeader = new OrderHeader
                {
                    Id = 1,
                    Name = "New Name",
                    PhoneNumber = "98765",
                    City = "New City",
                    Country = "New Country",
                    StreetAddress = "New Street",
                    PostalCode = "99999",
                    Carrier = "DHL",
                    TrackingNumber = "TRACK123"
                }
            };

            _orderService.UpdateOrder(updatedOrderVM);

            var updatedOrder = _dbContext.OrderHeaders.Find(1);
            Assert.That(updatedOrder, Is.Not.Null);
            Assert.That(updatedOrder.Name, Is.EqualTo("New Name"));
            Assert.That(updatedOrder.PhoneNumber, Is.EqualTo("98765"));
            Assert.That(updatedOrder.City, Is.EqualTo("New City"));
            Assert.That(updatedOrder.Country, Is.EqualTo("New Country"));
            Assert.That(updatedOrder.StreetAddress, Is.EqualTo("New Street"));
            Assert.That(updatedOrder.PostalCode, Is.EqualTo("99999"));
            Assert.That(updatedOrder.Carrier, Is.EqualTo("DHL"));
            Assert.That(updatedOrder.TrackingNumber, Is.EqualTo("TRACK123"));
        }
        #endregion

        #region GetOrderByStatusIntegrationTests
        [Test]
        public void GetOrdersByStatus_WhenAdmin_ReturnsAllOrdersWithGivenStatus()
        {
            var user1 = new User { Id = "user1", UserName = "User One", Name = "User One" };
            var user2 = new User { Id = "user2", UserName = "User Two", Name = "User Two" };
            _dbContext.Users.AddRange(user1, user2);

            var order1 = new OrderHeader { Id = 2, UserId = user1.Id, Name = "O1", PhoneNumber = "123", City = "C1", Country = "X", StreetAddress = "S", PostalCode = "111", OrderDate = DateTime.UtcNow, ShippingDate = DateTime.UtcNow, PaymentDate = DateTime.UtcNow, PaymentDueDate = DateOnly.FromDateTime(DateTime.UtcNow), OrderStatus = VinylVibe.Utility.Details.StatusInProcess };
            var order2 = new OrderHeader { Id = 3, UserId = user2.Id, Name = "O2", PhoneNumber = "456", City = "C2", Country = "Y", StreetAddress = "T", PostalCode = "222", OrderDate = DateTime.UtcNow, ShippingDate = DateTime.UtcNow, PaymentDate = DateTime.UtcNow, PaymentDueDate = DateOnly.FromDateTime(DateTime.UtcNow), OrderStatus = VinylVibe.Utility.Details.StatusInProcess };
            _dbContext.OrderHeaders.AddRange(order1, order2);
            _dbContext.SaveChanges();

            var result = _orderService.GetOrdersByStatus("inprocess", user1.Id, isAdmin: true).ToList();

            Assert.That(result.Count, Is.EqualTo(2)); 
            Assert.That(result.All(x => x.OrderStatus == VinylVibe.Utility.Details.StatusInProcess));
        }
        [Test]
        public void GetOrdersByStatus_WhenNotAdmin_ReturnsOnlyUserOrders()
        {
            var user1 = new User { Id = "user1", UserName = "User One", Name = "User One" };
            var user2 = new User { Id = "user2", UserName = "User Two", Name = "User Two" };
            _dbContext.Users.AddRange(user1, user2);

            var order1 = new OrderHeader { Id = 3, UserId = user1.Id, Name = "O1", PhoneNumber = "123", City = "C1", Country = "X", StreetAddress = "S", PostalCode = "111", OrderDate = DateTime.UtcNow, ShippingDate = DateTime.UtcNow, PaymentDate = DateTime.UtcNow, PaymentDueDate = DateOnly.FromDateTime(DateTime.UtcNow), OrderStatus = VinylVibe.Utility.Details.StatusApproved };
            var order2 = new OrderHeader { Id = 4, UserId = user2.Id, Name = "O2", PhoneNumber = "456", City = "C2", Country = "Y", StreetAddress = "T", PostalCode = "222", OrderDate = DateTime.UtcNow, ShippingDate = DateTime.UtcNow, PaymentDate = DateTime.UtcNow, PaymentDueDate = DateOnly.FromDateTime(DateTime.UtcNow), OrderStatus = VinylVibe.Utility.Details.StatusApproved };
            _dbContext.OrderHeaders.AddRange(order1, order2);
            _dbContext.SaveChanges();

            var result = _orderService.GetOrdersByStatus("approved", user1.Id, isAdmin: false).ToList();

            Assert.That(result.Count, Is.EqualTo(1)); 
            Assert.That(result.First().UserId, Is.EqualTo(user1.Id));
        }

        [Test]
        public void GetOrdersByStatus_WithCompletedStatus_ReturnsOnlyShippedOrders()
        {
            var user = new User { Id = "userCompleted", UserName = "Completed User" , Name = "Completed User" };
            _dbContext.Users.Add(user);

            var order1 = new OrderHeader
            {
                Id = 7,
                UserId = user.Id,
                Name = "O1",
                PhoneNumber = "123",
                City = "C1",
                Country = "X",
                StreetAddress = "S",
                PostalCode = "111",
                OrderDate = DateTime.UtcNow,
                ShippingDate = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow,
                OrderStatus = VinylVibe.Utility.Details.StatusShipped 
            };

            var order2 = new OrderHeader
            {
                Id = 8,
                UserId = user.Id,
                Name = "O2",
                PhoneNumber = "456",
                City = "C2",
                Country = "Y",
                StreetAddress = "T",
                PostalCode = "222",
                OrderDate = DateTime.UtcNow,
                ShippingDate = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow,
                OrderStatus = VinylVibe.Utility.Details.StatusInProcess 
            };

            _dbContext.OrderHeaders.AddRange(order1, order2);
            _dbContext.SaveChanges();

            var result = _orderService.GetOrdersByStatus("completed", user.Id, isAdmin: false).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().OrderStatus, Is.EqualTo(VinylVibe.Utility.Details.StatusShipped));
        }
        #endregion


    }
}
