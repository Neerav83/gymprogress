using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProgress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workout_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_templates_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_template_exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SuggestedSets = table.Column<int>(type: "integer", nullable: true),
                    SuggestedWeight = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    SuggestedRepsMin = table.Column<int>(type: "integer", nullable: true),
                    SuggestedRepsMax = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_template_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_template_exercises_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_template_exercises_workout_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "workout_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workout_template_exercises_ExerciseId",
                table: "workout_template_exercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_template_exercises_TemplateId_ExerciseId",
                table: "workout_template_exercises",
                columns: new[] { "TemplateId", "ExerciseId" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_templates_UserId_Name",
                table: "workout_templates",
                columns: new[] { "UserId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workout_template_exercises");

            migrationBuilder.DropTable(
                name: "workout_templates");
        }
    }
}
