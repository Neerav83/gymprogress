using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProgress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAndBodyMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ProfileImageUrl to users table
            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "users",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            // Create body_metrics table
            migrationBuilder.CreateTable(
                name: "body_metrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    ChestCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    WaistCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    HipsCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    ArmCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    ThighCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_body_metrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_body_metrics_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_body_metrics_UserId",
                table: "body_metrics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_body_metrics_UserId_Date",
                table: "body_metrics",
                columns: new[] { "UserId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "body_metrics");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "users");
        }
    }
}
