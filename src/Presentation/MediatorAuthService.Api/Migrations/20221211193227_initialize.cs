using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediatorAuthService.Api.Migrations
{
    public partial class initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "CreatedUserId", "DeletedDate", "DeletedUserId", "Email", "IsActive", "ModifiedDate", "ModifiedUserId", "Name", "Password", "RefreshToken", "Surname" },
                values: new object[] { new Guid("d0bfa391-a604-4049-a868-359091461e46"), new DateTime(2022, 12, 11, 19, 32, 27, 619, DateTimeKind.Utc).AddTicks(3872), new Guid("d0bfa391-a604-4049-a868-359091461e46"), null, null, "admin@gmail.com", true, null, null, "Admin", "AJP3f/ZqBr+xHwFkTPBkwVO+BgcrDmRe5l5lgSzKy993YYd01unheSSpwWbeaCyLFg==", "ACE4OSv8s/cKB7sTd3dbjDBDGtuF9H1g/padyLEz9HZ2zIvS6wMC97ddpbxUGPb0QQ==", "Admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
