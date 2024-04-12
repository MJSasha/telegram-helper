using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParrentCategoryId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "ParrentCategoryId",
                table: "Categories",
                newName: "ParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_ParrentCategoryId",
                table: "Categories",
                newName: "IX_Categories_ParentCategoryId");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "Notes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_AuthorId",
                table: "Notes",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Users_AuthorId",
                table: "Notes",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Users_AuthorId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Notes_AuthorId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "ParentCategoryId",
                table: "Categories",
                newName: "ParrentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                newName: "IX_Categories_ParrentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParrentCategoryId",
                table: "Categories",
                column: "ParrentCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
