using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateAuthorizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorizationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Capabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationGroupCapabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorizationGroupId = table.Column<int>(type: "int", nullable: false),
                    CapabilityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationGroupCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationGroupCapabilities_AuthorizationGroups_AuthorizationGroupId",
                        column: x => x.AuthorizationGroupId,
                        principalTable: "AuthorizationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizationGroupCapabilities_Capabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "Capabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAuthorizationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorizationGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthorizationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuthorizationGroups_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAuthorizationGroups_AuthorizationGroups_AuthorizationGroupId",
                        column: x => x.AuthorizationGroupId,
                        principalTable: "AuthorizationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGroupCapabilities_AuthorizationGroupId_CapabilityId",
                table: "AuthorizationGroupCapabilities",
                columns: new[] { "AuthorizationGroupId", "CapabilityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGroupCapabilities_CapabilityId",
                table: "AuthorizationGroupCapabilities",
                column: "CapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGroups_Name",
                table: "AuthorizationGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Capabilities_Name",
                table: "Capabilities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthorizationGroups_AuthorizationGroupId",
                table: "UserAuthorizationGroups",
                column: "AuthorizationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthorizationGroups_UserId_AuthorizationGroupId",
                table: "UserAuthorizationGroups",
                columns: new[] { "UserId", "AuthorizationGroupId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationGroupCapabilities");

            migrationBuilder.DropTable(
                name: "UserAuthorizationGroups");

            migrationBuilder.DropTable(
                name: "Capabilities");

            migrationBuilder.DropTable(
                name: "AuthorizationGroups");
        }
    }
}
