using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockerAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    BlockedAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Accounts_BlockedAccountId",
                        column: x => x.BlockedAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Blocks_Accounts_BlockerAccountId",
                        column: x => x.BlockerAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Follows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FollowerAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowedAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Follows_Accounts_FollowedAccountId",
                        column: x => x.FollowedAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Follows_Accounts_FollowerAccountId",
                        column: x => x.FollowerAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MuterAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    MutedAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mutes_Accounts_MutedAccountId",
                        column: x => x.MutedAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mutes_Accounts_MuterAccountId",
                        column: x => x.MuterAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockedAccountId",
                table: "Blocks",
                column: "BlockedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockerAccountId",
                table: "Blocks",
                column: "BlockerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockerAccountId_BlockedAccountId",
                table: "Blocks",
                columns: new[] { "BlockerAccountId", "BlockedAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_CreatedAt",
                table: "Blocks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_CreatedAt",
                table: "Follows",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowedAccountId",
                table: "Follows",
                column: "FollowedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerAccountId",
                table: "Follows",
                column: "FollowerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerAccountId_FollowedAccountId",
                table: "Follows",
                columns: new[] { "FollowerAccountId", "FollowedAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mutes_CreatedAt",
                table: "Mutes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Mutes_MutedAccountId",
                table: "Mutes",
                column: "MutedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Mutes_MuterAccountId",
                table: "Mutes",
                column: "MuterAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Mutes_MuterAccountId_MutedAccountId",
                table: "Mutes",
                columns: new[] { "MuterAccountId", "MutedAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "Follows");

            migrationBuilder.DropTable(
                name: "Mutes");
        }
    }
}
