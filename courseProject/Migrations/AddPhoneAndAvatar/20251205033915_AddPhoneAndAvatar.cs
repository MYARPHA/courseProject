using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace courseProject.Migrations.AddPhoneAndAvatar
{
    /// <inheritdoc />
    public partial class AddPhoneAndAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_assignments");

            migrationBuilder.DropTable(
                name: "request_statuses");

            migrationBuilder.DropTable(
                name: "RequestItemEntities");

            migrationBuilder.DropTable(
                name: "RequestEntities");

            migrationBuilder.AddColumn<string>(
                name: "avatar_path",
                table: "users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_path",
                table: "users");

            migrationBuilder.CreateTable(
                name: "request_statuses",
                columns: table => new
                {
                    status_entity_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.status_entity_id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequestEntities",
                columns: table => new
                {
                    RequestEntityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AssignedTo = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CustomerEmail = table.Column<string>(type: "longtext", nullable: false),
                    CustomerName = table.Column<string>(type: "longtext", nullable: false),
                    CustomerPhone = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestEntities", x => x.RequestEntityId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "admin_assignments",
                columns: table => new
                {
                    admin_assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    admin_user_id = table.Column<int>(type: "int", nullable: true),
                    request_entity_id = table.Column<int>(type: "int", nullable: false),
                    admin_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.admin_assignment_id);
                    table.ForeignKey(
                        name: "admin_assignments_ibfk_1",
                        column: x => x.admin_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "admin_assignments_ibfk_2",
                        column: x => x.request_entity_id,
                        principalTable: "RequestEntities",
                        principalColumn: "RequestEntityId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequestItemEntities",
                columns: table => new
                {
                    RequestItemEntityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RequestEntityId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: true)
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
                name: "IX_admin_assignments_admin_user_id",
                table: "admin_assignments",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "request_entity_id",
                table: "admin_assignments",
                column: "request_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_status_name",
                table: "request_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestItemEntities_RequestEntityId",
                table: "RequestItemEntities",
                column: "RequestEntityId");
        }
    }
}
