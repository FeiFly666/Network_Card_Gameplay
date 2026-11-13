using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    public partial class UserSpecialEffects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSpecialEffects",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SpecialEffectDataID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EquippedCharacterDataID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SlotIndex = table.Column<int>(type: "int", nullable: true),
                    UnlockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSpecialEffects", x => new { x.UserID, x.SpecialEffectDataID });
                    table.CheckConstraint("CK_UserSpecialEffects_Equipment", "([EquippedCharacterDataID] IS NULL AND [SlotIndex] IS NULL) OR ([EquippedCharacterDataID] IS NOT NULL AND [SlotIndex] IS NOT NULL AND [SlotIndex] IN (0, 1))");
                    table.ForeignKey(
                        name: "FK_UserSpecialEffects_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSpecialEffects_UserID_EquippedCharacterDataID_SlotIndex",
                table: "UserSpecialEffects",
                columns: new[] { "UserID", "EquippedCharacterDataID", "SlotIndex" },
                unique: true,
                filter: "[EquippedCharacterDataID] IS NOT NULL AND [SlotIndex] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSpecialEffects");
        }
    }
}
