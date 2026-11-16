using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyBuddyHelper.Core.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BabyProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WeightInLbs = table.Column<double>(type: "REAL", nullable: false),
                    HeightInFt = table.Column<double>(type: "REAL", nullable: false),
                    WeightUpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HeightUpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastFeed = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSleep = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabyProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssociatedBabyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TaskPriority = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskName = table.Column<string>(type: "TEXT", nullable: false),
                    TaskDescription = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    TaskType = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    AppointmentLocation = table.Column<string>(type: "TEXT", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AppointmentStartTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    AppointmentEndTime = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_BabyProfiles_AssociatedBabyId",
                        column: x => x.AssociatedBabyId,
                        principalTable: "BabyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VaccinationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BabyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VaccineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalDoses = table.Column<int>(type: "INTEGER", nullable: true),
                    CompletedDoses = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAdministered = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextDose = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaccinationRecords_BabyProfiles_BabyId",
                        column: x => x.BabyId,
                        principalTable: "BabyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssociatedBabyId",
                table: "Tasks",
                column: "AssociatedBabyId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_BabyId_VaccineId",
                table: "VaccinationRecords",
                columns: new[] { "BabyId", "VaccineId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "VaccinationRecords");

            migrationBuilder.DropTable(
                name: "BabyProfiles");
        }
    }
}
