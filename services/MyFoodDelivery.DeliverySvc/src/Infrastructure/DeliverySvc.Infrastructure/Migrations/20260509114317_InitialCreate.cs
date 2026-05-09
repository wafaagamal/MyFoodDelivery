using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySvc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PickupAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PickupLatitude = table.Column<double>(type: "float", nullable: false),
                    PickupLongitude = table.Column<double>(type: "float", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeliveryLatitude = table.Column<double>(type: "float", nullable: false),
                    DeliveryLongitude = table.Column<double>(type: "float", nullable: false),
                    DeliveryInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Riders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    LastLocationUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastOnlineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastOfflineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalDeliveries = table.Column<int>(type: "int", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    RatingCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Riders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTasks_OrderId",
                table: "DeliveryTasks",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTasks_RiderId",
                table: "DeliveryTasks",
                column: "RiderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTasks_Status",
                table: "DeliveryTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Riders_IsOnline",
                table: "Riders",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_Riders_Status",
                table: "Riders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryTasks");

            migrationBuilder.DropTable(
                name: "Riders");
        }
    }
}
