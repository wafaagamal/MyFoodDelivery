using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderingSvc.Infrastructure.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddRiderInfoToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RiderLatitude",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RiderLongitude",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiderName",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiderPhone",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiderLatitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RiderLongitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RiderName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RiderPhone",
                table: "Orders");
        }
    }
}
