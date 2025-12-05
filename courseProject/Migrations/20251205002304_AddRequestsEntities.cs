using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace courseProject.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            // Create only requests tables here. Other tables already exist in the database
            migrationBuilder.CreateTable(
                name: "RequestEntities",
                columns: table => new
                {
                    RequestEntityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CustomerName = table.Column<string>(type: "longtext", nullable: false),
                    CustomerEmail = table.Column<string>(type: "longtext", nullable: false),
                    CustomerPhone = table.Column<string>(type: "longtext", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    AssignedTo = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestEntities", x => x.RequestEntityId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequestItemEntities",
                columns: table => new
                {
                    RequestItemEntityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RequestEntityId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestItemEntities", x => x.RequestItemEntityId);
                    table.ForeignKey(
                        name: "FK_RequestItemEntities_RequestEntities_RequestEntityId",
                        column: x => x.RequestEntityId,
                        principalTable: "RequestEntities",
                        principalColumn: "RequestEntityId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RequestItemEntities_RequestEntityId",
                table: "RequestItemEntities",
                column: "RequestEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "RequestItemEntities");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "RequestEntities");

            migrationBuilder.DropTable(
                name: "service_categories");
        }
    }
}
