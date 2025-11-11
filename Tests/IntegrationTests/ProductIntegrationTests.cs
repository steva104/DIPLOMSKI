using Microsoft.AspNetCore.Http;
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
    public class ProductIntegrationTests
    {

        private ApplicationDbContext _dbContext;
        private IUnitOfWork _unitOfWork;
        private ProductService _productService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                              .UseInMemoryDatabase(databaseName: $"{Guid.NewGuid().ToString()}").Options;

            _dbContext = new ApplicationDbContext(options);

            
            SeedData(_dbContext);
            _unitOfWork = new UnitOfWork(_dbContext);
            _productService = new ProductService(_unitOfWork);



        }
        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }
        private void SeedData(ApplicationDbContext dbContext)
        {


            var genre1 = new Genre { Id = 1, Name = "Rock" };
            var genre2 = new Genre { Id = 2, Name = "Jazz" };
            var genre3 = new Genre { Id = 3, Name = "Pop" };
            dbContext.Genres.AddRange(genre1, genre2, genre3);

            var product1 = new Product
            {
                Id = 1,
                UPC = "111111",
                Description = "Cool guitar album",
                Title = "Guitar Album",
                Artist = "Rock Band",
                GenreId = genre1.Id,
                Genre = genre1,

            };

            var product2 = new Product
            {
                Id = 2,
                UPC = "222222",
                Description = "Cool jazz album",
                Title = "Jazz Classics",
                Artist = "Jazz Artist",
                GenreId = genre2.Id,
                Genre = genre2,

            };

            var product3 = new Product
            {
                Id = 3,
                UPC = "333333",
                Description = "Cool pop album",
                Title = "Pop Hits",     
                Artist = "Pop Star",              
                GenreId = genre3.Id,
                Genre = genre3,
            };

            dbContext.Products.AddRange(product1, product2, product3);

            dbContext.SaveChanges();

            foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

        }


        #region ProductIntegrationTests
        [Test]
        public void GetAllProducts_ShouldReturnAllProductsWithGenres()
        {
            var products = _productService.GetAllProducts().ToList();

            Assert.That(products.Count, Is.EqualTo(3));

            var p1 = products.First(p => p.Title.Contains("Guitar"));
            Assert.That(p1.Genre, Is.Not.Null);
            Assert.That(p1.Genre.Name, Is.EqualTo("Rock"));

            var p2 = products.First(p => p.Title.Contains("Jazz"));
            Assert.That(p2.Genre, Is.Not.Null);
            Assert.That(p2.Genre.Name, Is.EqualTo("Jazz"));

            var p3 = products.First(p => p.Title.Contains("Pop"));
            Assert.That(p3.Genre, Is.Not.Null);
            Assert.That(p3.Genre.Name, Is.EqualTo("Pop"));
        }

        [Test]
        public void GetAllProducts_WhenNoProductsExist_ShouldReturnEmptyList()
        {
            foreach (var p in _dbContext.Products) 
            {
                _dbContext.Products.Remove(p);
            }    
            _dbContext.SaveChanges();

            var products = _productService.GetAllProducts().ToList();

            Assert.That(products, Is.Not.Null);
            Assert.That(products.Count, Is.EqualTo(0));
        }
        #endregion

        #region ProductViewModelIntegrationTest
        [Test]
        public void GetProductViewModel_WhenIdExists_ReturnsProductWithGenreList()
        {
            int productId = 1;

            var result = _productService.GetProductViewModel(productId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Product, Is.Not.Null);
            Assert.That(result.Product.Id, Is.EqualTo(productId));
            Assert.That(result.Product.Title, Is.EqualTo("Guitar Album"));
            Assert.That(result.GenreList.Count(), Is.EqualTo(3));
            Assert.That(result.Product.Genre, Is.Null); 
        }
        #endregion

        #region UpsertIntegrationTests
        [Test]
        public void UpsertProduct_WhenNewProductWithoutImage_ReturnsFalse()
        {
            var newProduct = new Product
            {
                Title = "New Album",
                Artist = "New Artist",
                Description = "Some cool album",
                UPC = "444444",
                Year = 2025,
                ListPrice = 100,
                Price = 90,
                Price5 = 80,
                Price10 = 70,
                GenreId = 1
            };

            var vm = new ProductViewModel
            {
                Product = newProduct
            };

            var result = _productService.UpsertProduct(vm, null, "wwwroot");

            Assert.That(result, Is.False);
        }
        [Test]
        public void UpsertProduct_WhenNewProductWithImage_SetsImageUrlAndReturnsTrue()
        {
            _dbContext.ChangeTracker.Clear();
            var newProduct = new Product
            {
                Title = "Album with Image",
                Artist = "Some Artist",
                Description = "Some cool album",
                UPC = "555555",
                Year = 2025,
                ListPrice = 200,
                Price = 180,
                Price5 = 170,
                Price10 = 160,
                GenreId = 1,
                Genre = null
            };

            var vm = new ProductViewModel
            {
                Product = newProduct
            };

            var content = "fake image content";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.jpg");

            var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var productPath = Path.Combine(rootPath, "images", "product");
            Directory.CreateDirectory(productPath);

            var result = _productService.UpsertProduct(vm, file, rootPath);

            Assert.That(result, Is.True);
            Assert.That(vm.Product.ImageUrl, Does.StartWith(@"\images\product\"));

            Directory.Delete(rootPath, true);
        }
        [Test]
        public void UpsertProduct_WhenUpdatingExistingProduct_UpdatesInDatabase()
        {
            var product = _dbContext.Products.First();
            var vm = new ProductViewModel
            {
                Product = product
            };
            vm.Product.Title = "Updated Title";
            vm.Product.Price = 150;
       
            var result = _productService.UpsertProduct(vm, null, "wwwroot");

            var updated = _dbContext.Products.Find(product.Id);

            Assert.That(result, Is.True);
            Assert.That(updated.Title, Is.EqualTo("Updated Title"));
        }
        #endregion

        #region DeleteIntegrationTests
        [Test]
        public void DeleteProduct_WhenProductDoesNotExist_ReturnsFalse()
        {
            var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(rootPath);

            var result = _productService.DeleteProduct(999, rootPath);

            Assert.That(result, Is.False);

            Directory.Delete(rootPath, true);
        }

        [Test]
        public void DeleteProduct_WhenProductIsInOrder_ReturnsFalse()
        {
            var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(rootPath);

            var product = _dbContext.Products.First();
            _dbContext.OrderDetails.Add(new OrderDetails { ProductId = product.Id, Count = 1, Price = 10 });
            _dbContext.SaveChanges();

            var result = _productService.DeleteProduct(product.Id, rootPath);

            Assert.That(result, Is.False);

            Directory.Delete(rootPath, true);
        }
        [Test]
        public void DeleteProduct_WhenProductExistsAndNotInOrder_DeletesAndReturnsTrue()
        {
            var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var productPath = Path.Combine(rootPath, "images", "product");
            Directory.CreateDirectory(productPath);

            var product = new Product
            {
                Id = 4,
                Title = "Album to Delete",
                Description = "This album is about to be deleted",
                Artist = "Artist",
                UPC = "999999",
                Year = 2025,
                ListPrice = 100,
                Price = 90,
                Price5 = 80,
                Price10 = 70,
                GenreId = 1,
                ImageUrl = @"\images\product\toDelete.jpg"
            };

            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            var filePath = Path.Combine(productPath, "toDelete.jpg");
            File.WriteAllText(filePath, "fake content");
            Assert.That(File.Exists(filePath), Is.True);

            var result = _productService.DeleteProduct(product.Id, rootPath);

            Assert.That(result, Is.True);
            Assert.That(_dbContext.Products.Find(product.Id), Is.Null);
            Assert.That(File.Exists(filePath), Is.False);

            Directory.Delete(rootPath, true);
        }
        #endregion




    }
}
