using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiapGamesService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TableGameEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameCreatedEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCreatedEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameChangedEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    OldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NewName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NewDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    OldPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    NewPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    OldGenre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NewGenre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameChangedEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameChangedEvent_GameCreatedEvent_GameId",
                        column: x => x.GameId,
                        principalTable: "GameCreatedEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameChangedEvent_GameId_ChangedAt",
                table: "GameChangedEvent",
                columns: new[] { "GameId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GameCreatedEvent_Genre",
                table: "GameCreatedEvent",
                column: "Genre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameChangedEvent");

            migrationBuilder.DropTable(
                name: "GameCreatedEvent");
        }
    }
}
