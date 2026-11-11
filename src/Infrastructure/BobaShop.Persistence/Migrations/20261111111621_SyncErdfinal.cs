using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BobaShop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncErdfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "Vouchers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PayOSOrderCode",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.AddColumn<int>(
                name: "TableID",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiningTables",
                columns: table => new
                {
                    TableID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiningTables", x => x.TableID);
                    table.CheckConstraint("CK_DiningTables_Capacity", "\"Capacity\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "VoucherRedemptions",
                columns: table => new
                {
                    RedemptionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoucherID = table.Column<int>(type: "integer", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    OrderID = table.Column<int>(type: "integer", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherRedemptions", x => x.RedemptionID);
                    table.ForeignKey(
                        name: "FK_VoucherRedemptions_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherRedemptions_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherRedemptions_Vouchers_VoucherID",
                        column: x => x.VoucherID,
                        principalTable: "Vouchers",
                        principalColumn: "VoucherID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PayOSOrderCode",
                table: "Orders",
                column: "PayOSOrderCode",
                unique: true,
                filter: "\"PayOSOrderCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableID",
                table: "Orders",
                column: "TableID");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_TableNumber",
                table: "DiningTables",
                column: "TableNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherRedemptions_OrderID",
                table: "VoucherRedemptions",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherRedemptions_UserID",
                table: "VoucherRedemptions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherRedemptions_VoucherID_OrderID",
                table: "VoucherRedemptions",
                columns: new[] { "VoucherID", "OrderID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DiningTables_TableID",
                table: "Orders",
                column: "TableID",
                principalTable: "DiningTables",
                principalColumn: "TableID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DiningTables_TableID",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DiningTables");

            migrationBuilder.DropTable(
                name: "VoucherRedemptions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PayOSOrderCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TableID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PayOSOrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TableID",
                table: "Orders");
        }
    }
}
