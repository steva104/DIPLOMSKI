using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VinylVibe.Models;

namespace VinylVibe.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {
            
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companies {  get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<User> Users {  get; set; }

        public DbSet<OrderHeader > OrderHeaders { get; set; } 
        public DbSet<OrderDetails> OrderDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Genre>().HasData(
                new Genre { Id = 1, Name = "Rock", DisplayOrder = 1 },
                new Genre { Id = 2, Name = "Pop", DisplayOrder = 2},
                new Genre { Id = 3, Name = "Hip Hop", DisplayOrder = 3 },
                new Genre { Id = 4, Name = "Country", DisplayOrder = 4 }
                );


            modelBuilder.Entity<Company>().HasData(
     new Company { Id = 5, Name = "Harmony Records", StreetAddress = "123 Melody St", City = "Nashville", Country = "USA", PostalCode = "37201", PhoneNumber = "+1 615-555-1234" },
     new Company { Id = 6, Name = "BeatWave Studios", StreetAddress = "456 Rhythm Ave", City = "Los Angeles", Country = "USA", PostalCode = "90001", PhoneNumber = "+1 310-555-5678" },
     new Company { Id = 7, Name = "Symphony Productions", StreetAddress = "789 Crescendo Blvd", City = "London", Country = "UK", PostalCode = "W1D 3QJ", PhoneNumber = "+44 20 7946 0123" }
 );

            modelBuilder.Entity<Product>().HasData(
                 new Product
                 {
                     Id = 1,
                     Title = "Thriller",
                     Description = "Best-selling album of all time by Michael Jackson",
                     UPC = "123456789012",
                     Artist = "Michael Jackson",
                     Year = 1982,
                     ListPrice = 29,
                     Price = 25,
                     Price5 = 22,
                     Price10 = 19,
                     GenreId=9,
                     ImageUrl=""
                 },
                new Product
                {
             Id = 2,
        Title = "Back in Black",
        Description = "Legendary rock album by AC/DC",
        UPC = "987654321098",
        Artist = "AC/DC",
        Year = 1980,
        ListPrice = 24,
        Price = 21,
        Price5 = 18,
        Price10 = 15,
                    GenreId = 5,
                    ImageUrl = ""
                },
    new Product
    {
        Id = 3,
        Title = "The Dark Side of the Moon",
        Description = "Iconic progressive rock album by Pink Floyd",
        UPC = "112233445566",
        Artist = "Pink Floyd",
        Year = 1973,
        ListPrice = 27,
        Price = 24,
        Price5 = 21,
        Price10 = 18,
        GenreId = 1,
        ImageUrl = ""
    },
    new Product
    {
        Id = 4,
        Title = "Abbey Road",
        Description = "Classic rock album by The Beatles",
        UPC = "223344556677",
        Artist = "The Beatles",
        Year = 1969,
        ListPrice = 26,
        Price = 23,
        Price5 = 20,
        Price10 = 17,
        GenreId = 1,
        ImageUrl = ""
    },
    new Product
    {
        Id = 5,
        Title = "Nevermind",
        Description = "Grunge-defining album by Nirvana",
        UPC = "334455667788",
        Artist = "Nirvana",
        Year = 1991,
        ListPrice = 22,
        Price = 19,
        Price5 = 16,
        Price10 = 14,
        GenreId = 1,
        ImageUrl = ""
    }



                );
        }

    }
}
