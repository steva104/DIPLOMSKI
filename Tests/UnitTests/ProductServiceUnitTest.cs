using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using VinylVibe.BusinessLogic;
using VinylVibe.DataAccess.Repository.IRepository;
using VinylVibe.Models;
using VinylVibe.Models.ViewModel;

namespace Tests
{
    public class ProductServiceUnitTest
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepo;
        private Mock<IGenreRepository> _mockGenreRepo;
        private Mock<IOrderDetailsRepository> _mockOrderDetailsRepo;
        private ProductService _productService;
        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockGenreRepo = new Mock<IGenreRepository>();
            _mockOrderDetailsRepo = new Mock<IOrderDetailsRepository>();

            var genres = new List<Genre>
        {
            new Genre { Id = 1, Name = "Pop" },
            new Genre { Id = 2, Name = "Rock" }
        };

            _mockGenreRepo.Setup(r => r.GetAll(
                            It.IsAny<Expression<Func<Genre, bool>>>(),
                            It.IsAny<string>()))
                            .Returns((Expression<Func<Genre, bool>> filter, string includeProperties) =>
                            genres.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Product).Returns(_mockProductRepo.Object);
            _mockUnitOfWork.Setup(u => u.Genre).Returns(_mockGenreRepo.Object);
            _mockUnitOfWork.Setup(u => u.OrderDetails).Returns(_mockOrderDetailsRepo.Object);

            var product = new Product { Id = 1, Title = "Thriller" };
            _mockProductRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), null, false))
                            .Returns(product);

            _productService = new ProductService(_mockUnitOfWork.Object);
        }

        #region GetProductViewModelTests
        [Test]
        public void GetProductViewModel_IdIsNull_ReturnsEmptyProductWithGenres()
        {
            var result = _productService.GetProductViewModel(null);

            Assert.That(result.Product, Is.Not.Null);
            Assert.That(result.Product.Id, Is.EqualTo(0));
            Assert.That(result.GenreList.Count(), Is.EqualTo(2));
        }
        [Test]
        public void GetProductViewModel_IdIsValid_ReturnsProductWithGenres()
        {
            var result = _productService.GetProductViewModel(1);

            Assert.That(result.Product, Is.Not.Null);
            Assert.That(result.Product.Id, Is.EqualTo(1));
            Assert.That(result.Product.Title, Is.EqualTo("Thriller"));
            Assert.That(result.GenreList.Count(), Is.EqualTo(2));
        }
        #endregion

        #region Update/InsertProductTests
        [Test]
        public void UpsertProduct_NewProductWithFile_AddsProductAndSaves()
        {
            var wwwRoot = Path.Combine(Path.GetTempPath(), "images", "product");
            Directory.CreateDirectory(wwwRoot);

            var productVM = new ProductViewModel
            {
                Product = new Product
                {
                    Id = 0,
                    Title = "Test Product"
                }
            };

            var fileMock = new Mock<IFormFile>();
            var content = "Fake file content";
            var fileName = "test.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            var result = _productService.UpsertProduct(productVM, fileMock.Object, Path.GetTempPath());

            Assert.That(result, Is.True);
            _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);

            var addedFile = Path.Combine(wwwRoot, Path.GetFileName(productVM.Product.ImageUrl));
            if (File.Exists(addedFile))
                File.Delete(addedFile);
            Directory.Delete(wwwRoot, true);
        }
        [Test]
        public void UpsertProduct_NewProductWithoutFile_ReturnsFalse()
        {
            var productVM = new ProductViewModel { Product = new Product { Id = 0, ImageUrl = null } };

            var result = _productService.UpsertProduct(productVM, null, "");

            Assert.That(result, Is.False);
        }
        #endregion

        #region DeleteProductTests
        [Test]
        public void DeleteProduct_ProductNotInOrder_DeletedSuccessfully()
        {
            var product = new Product { Id = 1, ImageUrl = @"\images\product\test.png" };
            _mockProductRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
                            .Returns(product);

            _mockOrderDetailsRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OrderDetails, bool>>>(), It.IsAny<string>()))
                                 .Returns(new List<OrderDetails>().AsQueryable());

            var result = _productService.DeleteProduct(1, @"C:\Temp\");

            Assert.That(result, Is.True);
            _mockProductRepo.Verify(r => r.Delete(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
        [Test]
        public void DeleteProduct_ProductInOrder_ReturnsFalse()
        {
            var product = new Product { Id = 2, ImageUrl = @"\images\product\test2.png" };
            _mockProductRepo.Setup(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
                            .Returns(product);

            _mockOrderDetailsRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OrderDetails, bool>>>(), It.IsAny<string>()))
                                 .Returns(new List<OrderDetails> { new OrderDetails { ProductId = 2 } }.AsQueryable());

            var result = _productService.DeleteProduct(2, @"C:\Temp\");

            Assert.That(result, Is.False);
            _mockProductRepo.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }
        #endregion
    }
}
