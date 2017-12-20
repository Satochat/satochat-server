using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Satochat.Server.Migrations
{
    public partial class RemoveUserFriendRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFriendRequest");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFriendRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Expiry = table.Column<DateTimeOffset>(nullable: false),
                    FriendId = table.Column<int>(nullable: false),
                    InitiatorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFriendRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFriendRequest_Users_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFriendRequest_Users_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendRequest_FriendId",
                table: "UserFriendRequest",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendRequest_InitiatorId",
                table: "UserFriendRequest",
                column: "InitiatorId");
        }
    }
}
