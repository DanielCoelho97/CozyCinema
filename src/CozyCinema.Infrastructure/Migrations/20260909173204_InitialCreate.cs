using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozyCinema.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    LastPickerUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_LastPickerUserId",
                        column: x => x.LastPickerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SessionMembers",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMembers", x => new { x.SessionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SessionMembers_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WatchedMovies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TmdbMovieId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CoverUrl = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    Genres = table.Column<string>(type: "text", nullable: false),
                    Director = table.Column<string>(type: "text", nullable: true),
                    WatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchedMovies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchedMovies_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchedMovies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoundMovies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingRoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuggestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TmdbMovieId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CoverUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundMovies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoundMovies_Users_SuggestedByUserId",
                        column: x => x.SuggestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundMovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_RoundMovies_RoundMovieId",
                        column: x => x.RoundMovieId,
                        principalTable: "RoundMovies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VotingRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WinnerMovieId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotingRounds_RoundMovies_WinnerMovieId",
                        column: x => x.WinnerMovieId,
                        principalTable: "RoundMovies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VotingRounds_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoundMovies_SuggestedByUserId",
                table: "RoundMovies",
                column: "SuggestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundMovies_VotingRoundId",
                table: "RoundMovies",
                column: "VotingRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMembers_UserId",
                table: "SessionMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_LastPickerUserId",
                table: "Sessions",
                column: "LastPickerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_RoundMovieId_UserId",
                table: "Votes",
                columns: new[] { "RoundMovieId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingRounds_SessionId",
                table: "VotingRounds",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingRounds_WinnerMovieId",
                table: "VotingRounds",
                column: "WinnerMovieId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedMovies_SessionId",
                table: "WatchedMovies",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedMovies_UserId",
                table: "WatchedMovies",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoundMovies_VotingRounds_VotingRoundId",
                table: "RoundMovies",
                column: "VotingRoundId",
                principalTable: "VotingRounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoundMovies_Users_SuggestedByUserId",
                table: "RoundMovies");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_LastPickerUserId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_RoundMovies_VotingRounds_VotingRoundId",
                table: "RoundMovies");

            migrationBuilder.DropTable(
                name: "SessionMembers");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "WatchedMovies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VotingRounds");

            migrationBuilder.DropTable(
                name: "RoundMovies");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
