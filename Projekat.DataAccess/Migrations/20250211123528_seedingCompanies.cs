using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VinylVibe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class seedingCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "Country", "Name", "PhoneNumber", "PostalCode", "StreetAddress" },
                values: new object[,]
                {
                    { 5, "Nashville", "USA", "Harmony Records", "+1 615-555-1234", "37201", "123 Melody St" },
                    { 6, "Los Angeles", "USA", "BeatWave Studios", "+1 310-555-5678", "90001", "456 Rhythm Ave" },
                    { 7, "London", "UK", "Symphony Productions", "+44 20 7946 0123", "W1D 3QJ", "789 Crescendo Blvd" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
