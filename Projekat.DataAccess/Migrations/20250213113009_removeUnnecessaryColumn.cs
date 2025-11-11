using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinylVibe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removeUnnecessaryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "OrderHeaders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
