using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace courseProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_assignments",
                columns: table => new
                {
                    admin_assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    admin_user_id = table.Column<int>(type: "int", nullable: true),
                    admin_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    request_entity_id = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_admin_assignments_admin_user_id",
                table: "admin_assignments",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "request_entity_id",
                table: "admin_assignments",
                column: "request_entity_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_assignments");
        }
    }
}
