using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;

namespace Tests
{
    public class HomeServiceUnitTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepo;
        private Mock<IGenreRepository> _mockGenreRepo;
        private Mock<IShoppingCartRepository> _mockCartRepo;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private HomeService _homeService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockGenreRepo = new Mock<IGenreRepository>();
            _mockCartRepo = new Mock<IShoppingCartRepository>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var products = new List<Product>
        {
            new Product { Id = 1, Title = "Thriller", Artist = "Michael Jackson", Genre = new Genre { Name = "Pop" } },
            new Product { Id = 2, Title = "Back in Black", Artist = "ACDC", Genre = new Genre { Name = "Rock" } },
            new Product { Id = 3, Title = "Kind of Blue", Artist = "Miles Davis", Genre = new Genre { Name = "Jazz" } }
        }.AsQueryable();

            var genres = new List<Genre>
            {
                 new Genre { Name = "Pop" },
                 new Genre { Name = "Rock" },
                 new Genre { Name = "Jazz" }
            }.AsQueryable();


            _mockProductRepo.Setup(r => r.GetAll(
                 It.IsAny<Expression<Func<Product, bool>>>(),
                 It.IsAny<string>()
                    )).Returns(products);
            _mockGenreRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Genre, bool>>>(),
                It.IsAny<string>()
            )).Returns(genres);
            _mockProductRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()
            )).Returns((Expression<Func<Product, bool>> filter, string includeProperties, bool tracked) =>
               products.FirstOrDefault(filter.Compile())
            );
            _mockUnitOfWork.Setup(u => u.Product).Returns(_mockProductRepo.Object);
            _mockUnitOfWork.Setup(u => u.Genre).Returns(_mockGenreRepo.Object);
            _mockUnitOfWork.Setup(u => u.ShoppingCart).Returns(_mockCartRepo.Object);

            var mockSession = new Mock<ISession>();
            var mockContext = new DefaultHttpContext();
            mockContext.Session = mockSession.Object;
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockContext);

            _homeService = new HomeService(_mockUnitOfWork.Object, _mockHttpContextAccessor.Object);
        }


        #region GetFilteredProductTests
        [Test]
        public void GetFilteredProducts_NoFilters_ReturnsAll()
        {
            var result = _homeService.GetFilteredProducts("", "").ToList();
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetFilteredProducts_SearchByTitle_ReturnsCorrect()
        {
            var result = _homeService.GetFilteredProducts("Thriller", "").ToList();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Thriller"));
        }

        [Test]
        public void GetFilteredProducts_FilterByGenre_ReturnsOnlyGenre()
        {
            var result = _homeService.GetFilteredProducts("", "Jazz").ToList();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Genre.Name, Is.EqualTo("Jazz"));
        }

        [Test]
        public void GetFilteredProducts_SearchAndGenre_ReturnsMatching()
        {
            var result = _homeService.GetFilteredProducts("Kind", "Jazz").ToList();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Kind of Blue"));
        }

        [Test]
        public void GetFilteredProducts_NoMatch_ReturnsEmpty()
        {
            var result = _homeService.GetFilteredProducts("Unknown", "Pop").ToList();
            Assert.That(result.Count, Is.EqualTo(0));
        }
        #endregion

        #region GetGenresTest
        [Test]
        public void GetGenres_ReturnsAllGenres()
        {
            var result = _homeService.GetGenres().ToList();
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result, Does.Contain("Pop"));
            Assert.That(result, Does.Contain("Rock"));
            Assert.That(result, Does.Contain("Jazz"));
        }
        #endregion

        #region GetProductDetailsTests
        [Test]
        public void GetProductDetails_ProductExists_ReturnsShoppingCart()
        {
            int productId = 1;

            var result = _homeService.GetProductDetails(productId);

            Assert.That(result, !Is.Null);
            Assert.That(productId, Is.EqualTo(result.ProductId));
            Assert.That("Thriller", Is.EqualTo(result.Product.Title));
            Assert.That(1, Is.EqualTo(result.Count));
            Assert.That("Pop", Is.EqualTo(result.Product.Genre.Name));
        }
        [Test]
        public void GetProductDetails_ProductDoesNotExist_ReturnsNull()
        {

            int productId = 999;

            var result = _homeService.GetProductDetails(productId);

            Assert.That(result, Is.Null);
        }
        #endregion

        #region AddOrUpdateCartTests
        [Test]
        public void AddOrUpdateCart_CountZeroOrLess_ReturnsFalse()
        {
            var cart = new ShoppingCart { Count = 0, ProductId = 1 };

            var result = _homeService.AddOrUpdateCart(cart, "user1");

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void AddOrUpdateCart_ProductNotInCart_AddsNewCartItem()
        {
            var cart = new ShoppingCart { Count = 2, ProductId = 1 };

            _mockCartRepo.Setup(r => r.Get(
            It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<bool>()
            )).Returns((ShoppingCart)null);

            var result = _homeService.AddOrUpdateCart(cart, "user1");

            Assert.That(result, Is.EqualTo(true));
            _mockCartRepo.Verify(r => r.Add(It.IsAny<ShoppingCart>()), Times.Once);
            _mockCartRepo.Verify(r => r.Update(It.IsAny<ShoppingCart>()), Times.Never);
        }

        [Test]
        public void AddOrUpdateCart_ProductExists_UpdatesCartItem()
        {
            var existingCart = new ShoppingCart { Id = 10, Count = 1, ProductId = 1, UserId = "user1" };
            var cart = new ShoppingCart { Count = 2, ProductId = 1 };

            _mockCartRepo.Setup(r => r.Get(
            It.Is<Expression<Func<ShoppingCart, bool>>>(expr => true),
            It.IsAny<string>(),
            It.IsAny<bool>()
            )).Returns(existingCart);

            var result = _homeService.AddOrUpdateCart(cart, "user1");

            Assert.That(result, Is.EqualTo(true));
            _mockCartRepo.Verify(r => r.Update(It.Is<ShoppingCart>(c => c.Count == 3)), Times.Once);
            _mockCartRepo.Verify(r => r.Add(It.IsAny<ShoppingCart>()), Times.Never);
        }
        #endregion


    }
}
