using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaSimulator.Server.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNpcSimulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NpcProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NpcId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActivityState = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSimulatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextSimulationAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SimulationIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    SimulationVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NpcProfiles_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NpcActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NpcProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetPostId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TargetAccountId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Content = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: true),
                    Executed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NpcActions_NpcProfiles_NpcProfileId",
                        column: x => x.NpcProfileId,
                        principalTable: "NpcProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NpcInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NpcProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestKey = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Strength = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NpcInterests_NpcProfiles_NpcProfileId",
                        column: x => x.NpcProfileId,
                        principalTable: "NpcProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NpcPersonalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NpcProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Openness = table.Column<double>(type: "REAL", nullable: false),
                    Conscientiousness = table.Column<double>(type: "REAL", nullable: false),
                    Extraversion = table.Column<double>(type: "REAL", nullable: false),
                    Agreeableness = table.Column<double>(type: "REAL", nullable: false),
                    Neuroticism = table.Column<double>(type: "REAL", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcPersonalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NpcPersonalities_NpcProfiles_NpcProfileId",
                        column: x => x.NpcProfileId,
                        principalTable: "NpcProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NpcActions_Executed_ScheduledAt",
                table: "NpcActions",
                columns: new[] { "Executed", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NpcActions_NpcProfileId",
                table: "NpcActions",
                column: "NpcProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcActions_TargetAccountId",
                table: "NpcActions",
                column: "TargetAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcActions_TargetPostId",
                table: "NpcActions",
                column: "TargetPostId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcInterests_InterestKey",
                table: "NpcInterests",
                column: "InterestKey");

            migrationBuilder.CreateIndex(
                name: "IX_NpcInterests_NpcProfileId_InterestKey",
                table: "NpcInterests",
                columns: new[] { "NpcProfileId", "InterestKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NpcPersonalities_NpcProfileId",
                table: "NpcPersonalities",
                column: "NpcProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NpcProfiles_AccountId",
                table: "NpcProfiles",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NpcProfiles_IsActive_NextSimulationAt",
                table: "NpcProfiles",
                columns: new[] { "IsActive", "NextSimulationAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NpcProfiles_NextSimulationAt",
                table: "NpcProfiles",
                column: "NextSimulationAt");

            migrationBuilder.CreateIndex(
                name: "IX_NpcProfiles_NpcId",
                table: "NpcProfiles",
                column: "NpcId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NpcActions");

            migrationBuilder.DropTable(
                name: "NpcInterests");

            migrationBuilder.DropTable(
                name: "NpcPersonalities");

            migrationBuilder.DropTable(
                name: "NpcProfiles");
        }
    }
}
