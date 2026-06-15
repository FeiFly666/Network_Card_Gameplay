using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    public partial class AddFriendShips : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FriendShips",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserBID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RequiredByID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreateAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdateAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendShips", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendShips_UserAID_UserBID",
                table: "FriendShips",
                columns: new[] { "UserAID", "UserBID" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendShips");
        }
    }
}
