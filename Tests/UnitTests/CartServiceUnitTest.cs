using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net.Http;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;
using VinylVibe.Utility;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Tests.UnitTests
{
    public class CartServiceUnitTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IShoppingCartRepository> _mockCartRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IOrderHeaderRepository> _mockOrderHeaderRepo;
        private Mock<IOrderDetailsRepository> _mockOrderDetailsRepo;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private CartService _cartService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCartRepo = new Mock<IShoppingCartRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockOrderHeaderRepo = new Mock<IOrderHeaderRepository>();
            _mockOrderDetailsRepo = new Mock<IOrderDetailsRepository>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();


            var cartItems = new List<ShoppingCart>
    {
        new ShoppingCart
        {
            Id = 1,
            ProductId = 1,
            Count = 2,
            UserId = "user1",
            Product = new Product
            {
                Title = "Thriller",
                Price = 100,
                Price5 = 90,
                Price10 = 80
            }
        },
        new ShoppingCart
        {
            Id = 2,
            ProductId = 2,
            Count = 1,
            UserId = "user1",
            Product = new Product
            {
                Title = "Kind of Blue",
                Price = 200,
                Price5 = 180,
                Price10 = 160
            }
        }
    };

            var user = new VinylVibe.Models.User
            {
                Id = "user1",
                Name = "John Doe",
                StreetAddress = "123 Main St",
                PhoneNumber = "555-1234",
                City = "New York",
                Country = "USA",
                PostalCode = "10001"
            };

            _mockCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns((Expression<Func<ShoppingCart, bool>> filter, string includeProperties) =>
                {
                    if (filter == null) return cartItems.AsQueryable();
                    return cartItems.Where(filter.Compile()).AsQueryable();
                });

            _mockCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((Expression<Func<ShoppingCart, bool>> filter, string includeProperties, bool tracked) =>
                    cartItems.FirstOrDefault(filter.Compile()));

            _mockUserRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<VinylVibe.Models.User, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((Expression<Func<VinylVibe.Models.User, bool>> filter, string includeProperties, bool tracked) =>
                     filter.Compile()(user) ? user : null);

            _mockCartRepo.Setup(r => r.Delete(It.IsAny<ShoppingCart>()))
                .Callback<ShoppingCart>(cartToDelete =>
                {
                    cartItems.RemoveAll(c => c.Id == cartToDelete.Id);
                });

            _mockCartRepo.Setup(r => r.Update(It.IsAny<ShoppingCart>()))
                .Callback<ShoppingCart>(updatedCart =>
                {
                    var existing = cartItems.FirstOrDefault(c => c.Id == updatedCart.Id);
                    if (existing != null)
                    {
                        cartItems.Remove(existing);
                        cartItems.Add(updatedCart);
                    }
                });

            _mockOrderHeaderRepo.Setup(r => r.Add(It.IsAny<OrderHeader>()))
            .Callback<OrderHeader>(orderHeader =>
            {
                orderHeader.Id = 123;
            });

            _mockOrderDetailsRepo.Setup(r => r.Add(It.IsAny<OrderDetails>()))
                .Verifiable();


            _mockUnitOfWork.Setup(u => u.ShoppingCart).Returns(_mockCartRepo.Object);
            _mockUnitOfWork.Setup(u => u.User).Returns(_mockUserRepo.Object);
            _mockUnitOfWork.Setup(u => u.OrderHeader).Returns(_mockOrderHeaderRepo.Object);
            _mockUnitOfWork.Setup(u => u.OrderDetails).Returns(_mockOrderDetailsRepo.Object);
            _mockUnitOfWork.Setup(u => u.Save()).Verifiable();

            var sessionMock = new Mock<ISession>();
            var sessionValues = new Dictionary<string, byte[]>();

            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                       .Callback<string, byte[]>((key, value) => sessionValues[key] = value);

            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                       .Returns((string key, out byte[] value) =>
                       {
                           value = sessionValues.ContainsKey(key) ? sessionValues[key] : null;
                           return value != null;
                       });

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

            _cartService = new CartService(_mockUnitOfWork.Object, _mockHttpContextAccessor.Object);
        }

        #region GetShoppingCartDataTests
        [Test]
        public void GetShoppingCartData_WithNonExistentUserId_ReturnsEmptyViewModel()
        {

            var nonExistentUserId = "user999";

            var result = _cartService.GetShoppingCartData(nonExistentUserId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Is.Not.Null);
            Assert.That(result.Items.Count, Is.EqualTo(0));
            Assert.That(result.OrderHeader.OrderTotal, Is.EqualTo(0));
        }

        [Test]
        public void GetShoppingCartData_ItemsHaveCorrectPriceBasedOnQuantity()
        {

            var userId = "user1";


            var result = _cartService.GetShoppingCartData(userId);


            foreach (var item in result.Items)
            {
                var expectedPrice = _cartService.GetPriceBasedOnQuantity(item);
                Assert.That(item.Price, Is.EqualTo(expectedPrice));
            }
        }

        [Test]
        public void GetShoppingCartData_OrderTotalIsCorrectlyCalculated()
        {
            var userId = "user1";

            var result = _cartService.GetShoppingCartData(userId);

            var manualTotal = 0.0;
            foreach (var item in result.Items)
            {
                var itemPrice = _cartService.GetPriceBasedOnQuantity(item);
                manualTotal += itemPrice * item.Count;
            }

            Assert.That(result.OrderHeader.OrderTotal, Is.EqualTo(manualTotal));
        }
        #endregion

        #region GetChekoutDataTests

        [Test]
        public void GetCheckoutData_UserPropertiesAreCorrectlyMapped()
        {

            var userId = "user1";

            var result = _cartService.GetCheckoutData(userId);

            Assert.That(result.OrderHeader.Name, Is.EqualTo(result.OrderHeader.User.Name));
            Assert.That(result.OrderHeader.StreetAddress, Is.EqualTo(result.OrderHeader.User.StreetAddress));
            Assert.That(result.OrderHeader.PhoneNumber, Is.EqualTo(result.OrderHeader.User.PhoneNumber));
            Assert.That(result.OrderHeader.City, Is.EqualTo(result.OrderHeader.User.City));
            Assert.That(result.OrderHeader.Country, Is.EqualTo(result.OrderHeader.User.Country));
            Assert.That(result.OrderHeader.PostalCode, Is.EqualTo(result.OrderHeader.User.PostalCode));
        }
        [Test]
        public void GetCheckoutData_PriceCalculationIsCorrectForAllItems()
        {
            var userId = "user1";

            var result = _cartService.GetCheckoutData(userId);

            foreach (var item in result.Items)
            {
                var expectedPrice = _cartService.GetPriceBasedOnQuantity(item);
                Assert.That(item.Price, Is.EqualTo(expectedPrice));
            }
        }

        [Test]
        public void GetCheckoutData_OrderTotalIsSumOfAllItems()
        {
            var userId = "user1";

            var result = _cartService.GetCheckoutData(userId);

            var manualTotal = 0.0;
            foreach (var item in result.Items)
            {
                manualTotal += item.Price * item.Count;
            }

            Assert.That(result.OrderHeader.OrderTotal, Is.EqualTo(manualTotal));
        }
        [Test]
        public void GetCheckoutData_EmptyCart_ReturnsValidViewModelWithZeroTotal()
        {
            var emptyCartItems = new List<ShoppingCart>().AsQueryable();
            _mockCartRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<ShoppingCart, bool>>>(), It.IsAny<string>()))
                         .Returns(emptyCartItems);

            var userId = "user1";
            var result = _cartService.GetCheckoutData(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Is.Not.Null);
            Assert.That(result.Items.Count, Is.EqualTo(0));
            Assert.That(result.OrderHeader.OrderTotal, Is.EqualTo(0));
            Assert.That(result.OrderHeader.User, Is.Not.Null);
        }
        #endregion

        #region AddItemToCartTests
        [Test]
        public void AddItemToCart_ValidCartId_IncrementsCountAndSaves()
        {
            var cartId = 1;
            var shoppingCart = new ShoppingCart
            {
                Id = cartId,
                Count = 2,
                UserId = "user1"
            };

            _mockCartRepo.Setup(r => r.Get(
                    It.Is<Expression<Func<ShoppingCart, bool>>>(x => x.Compile()(shoppingCart)),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(shoppingCart);


            _cartService.AddItemToCart(cartId);

            Assert.That(shoppingCart.Count, Is.EqualTo(3));
            _mockCartRepo.Verify(r => r.Update(It.Is<ShoppingCart>(c => c.Count == 3)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        [Test]
        public void AddItemToCart_NonExistentCartId_DoesNotUpdateOrSave()
        {
            var nonExistentCartId = 999;

            _mockCartRepo.Setup(r => r.Get(
                    It.Is<Expression<Func<ShoppingCart, bool>>>(x =>
                        x.Compile()(new ShoppingCart { Id = nonExistentCartId })),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns((ShoppingCart)null);

            Assert.Throws<NullReferenceException>(() => _cartService.AddItemToCart(nonExistentCartId));

            _mockCartRepo.Verify(r => r.Update(It.IsAny<ShoppingCart>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }
        #endregion

        #region RemoveAndDecreaseTests
        [Test]
        public void RemoveCartItem_RemovesItemAndUpdatesSession_WhenItemExists()
        {

            int cartId = 1;
            int initialCount = _mockUnitOfWork.Object.ShoppingCart.GetAll(x => true).Count();


            _cartService.RemoveCartItem(cartId);

            var remainingItems = _mockUnitOfWork.Object.ShoppingCart.GetAll(x => true).ToList();


            Assert.That(remainingItems.All(c => c.Id != cartId), Is.True);
            Assert.That(remainingItems.Count, Is.EqualTo(initialCount - 1));


            _mockCartRepo.Verify(r => r.Delete(It.Is<ShoppingCart>(c => c.Id == cartId)), Times.Once);


            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);


        }
        [Test]
        public void DecreaseCartItem_WhenCountGreaterThanOne_ShouldDecreaseCount()
        {
            var cartId = 1;
            var initialCount = _mockUnitOfWork.Object.ShoppingCart
                .Get(x => x.Id == cartId, tracked: true).Count;

            _cartService.DecreaseCartItem(cartId);

            var updatedCart = _mockUnitOfWork.Object.ShoppingCart
                .Get(x => x.Id == cartId, tracked: true);

            Assert.That(updatedCart.Count, Is.EqualTo(initialCount - 1));
            _mockCartRepo.Verify(r => r.Update(It.IsAny<ShoppingCart>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        [Test]
        public void DecreaseCartItem_WhenCountIsOne_ShouldDeleteCartItem()
        {
            var cartId = 2;
            var userId = "user1";

            var initialCartCount = _mockUnitOfWork.Object.ShoppingCart
                .GetAll(x => x.UserId == userId).Count();


            _cartService.DecreaseCartItem(cartId);


            var cartAfterDeletion = _mockUnitOfWork.Object.ShoppingCart
                .Get(x => x.Id == cartId, tracked: true);
            Assert.That(cartAfterDeletion, Is.Null);

            var remainingCartItems = _mockUnitOfWork.Object.ShoppingCart
                .GetAll(x => x.UserId == userId).ToList();
            Assert.That(remainingCartItems.Count, Is.EqualTo(initialCartCount - 1));

            _mockCartRepo.Verify(r => r.Delete(It.Is<ShoppingCart>(c => c.Id == cartId)), Times.Once);


            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);


        }
        #endregion

        #region ProcessCheckoutTests
        [Test]
        public void ProcessCheckout_ForIndividualUser_ShouldProcessOrderWithImmediatePayment()
        {
            var userId = "user1";
            var cartViewModel = new ShoppingCartViewModel
            {
                OrderHeader = new OrderHeader()
            };

            var result = _cartService.ProcessCheckout(userId, cartViewModel);

            Assert.That(result, Is.EqualTo(123));
            Assert.That(cartViewModel.OrderHeader.PaymentStatus, Is.EqualTo(Details.PaymentStatusApproved));
            Assert.That(cartViewModel.OrderHeader.OrderStatus, Is.EqualTo(Details.StatusApproved));
            Assert.That(cartViewModel.OrderHeader.PaymentDate, Is.Not.Null);

            _mockOrderHeaderRepo.Verify(r => r.Add(It.IsAny<OrderHeader>()), Times.Once);
            _mockOrderDetailsRepo.Verify(r => r.Add(It.IsAny<OrderDetails>()), Times.Exactly(2));
            _mockCartRepo.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<ShoppingCart>>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.AtLeast(3));
        }
        [Test]
        public void ProcessCheckout_ForCompanyUser_ShouldProcessOrderWithDelayedPayment()
        {
            var userId = "user1";
            var cartViewModel = new ShoppingCartViewModel
            {
                OrderHeader = new OrderHeader()
            };

            var companyUser = new VinylVibe.Models.User
            {
                Id = "user1",
                Name = "John Doe",
                CompanyId = 123
            };

            _mockUserRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<VinylVibe.Models.User, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(companyUser);

            var result = _cartService.ProcessCheckout(userId, cartViewModel);

            Assert.That(result, Is.EqualTo(123));
            Assert.That(cartViewModel.OrderHeader.PaymentStatus, Is.EqualTo(Details.PaymentStatusDelayedPayment));
            Assert.That(cartViewModel.OrderHeader.OrderStatus, Is.EqualTo(Details.StatusApproved));
            Assert.That(cartViewModel.OrderHeader.PaymentDate, Is.Default);
        }
        #endregion
    }
}
