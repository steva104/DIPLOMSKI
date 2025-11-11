using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VinylVibe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addProductsToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UPC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Artist = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ListPrice = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Price5 = table.Column<double>(type: "float", nullable: false),
                    Price10 = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Artist", "Description", "ListPrice", "Price", "Price10", "Price5", "Title", "UPC", "Year" },
                values: new object[,]
                {
                    { 1, "Michael Jackson", "Best-selling album of all time by Michael Jackson", 29.0, 25.0, 19.0, 22.0, "Thriller", "123456789012", 1982 },
                    { 2, "AC/DC", "Legendary rock album by AC/DC", 24.0, 21.0, 15.0, 18.0, "Back in Black", "987654321098", 1980 },
                    { 3, "Pink Floyd", "Iconic progressive rock album by Pink Floyd", 27.0, 24.0, 18.0, 21.0, "The Dark Side of the Moon", "112233445566", 1973 },
                    { 4, "The Beatles", "Classic rock album by The Beatles", 26.0, 23.0, 17.0, 20.0, "Abbey Road", "223344556677", 1969 },
                    { 5, "Nirvana", "Grunge-defining album by Nirvana", 22.0, 19.0, 14.0, 16.0, "Nevermind", "334455667788", 1991 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
