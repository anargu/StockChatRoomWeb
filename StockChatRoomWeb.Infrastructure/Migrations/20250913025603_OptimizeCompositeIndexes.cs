using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockChatRoomWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ChatRoomId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_CreatedAt",
                table: "ChatMessages");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatRoomId_CreatedAt",
                table: "ChatMessages",
                columns: new[] { "ChatRoomId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_GlobalChat_CreatedAt",
                table: "ChatMessages",
                column: "CreatedAt",
                descending: new bool[0],
                filter: "\"ChatRoomId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ChatRoomId_CreatedAt",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_GlobalChat_CreatedAt",
                table: "ChatMessages");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatRoomId",
                table: "ChatMessages",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_CreatedAt",
                table: "ChatMessages",
                column: "CreatedAt",
                descending: new bool[0]);
        }
    }
}
