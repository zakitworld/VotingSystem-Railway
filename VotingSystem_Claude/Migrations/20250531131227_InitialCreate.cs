using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingSystem_Claude.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Elections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SchoolYear = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StudentId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Class = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    House = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Gender = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MaxSelectableOptions = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ElectionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Voters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoterCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HasVoted = table.Column<bool>(type: "INTEGER", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voters_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Class = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    ManifestoSummary = table.Column<string>(type: "TEXT", nullable: false),
                    PositionId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidates_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Candidates_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoterId = table.Column<int>(type: "INTEGER", nullable: false),
                    CandidateId = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ElectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Voters_VoterId",
                        column: x => x.VoterId,
                        principalTable: "Voters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_PositionId",
                table: "Candidates",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_StudentId",
                table: "Candidates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ElectionId",
                table: "Positions",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentId",
                table: "Students",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voters_StudentId",
                table: "Voters",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voters_VoterCode",
                table: "Voters",
                column: "VoterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_CandidateId",
                table: "Votes",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ElectionId",
                table: "Votes",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PositionId",
                table: "Votes",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_VoterId",
                table: "Votes",
                column: "VoterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "Voters");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Elections");
        }
    }
}
