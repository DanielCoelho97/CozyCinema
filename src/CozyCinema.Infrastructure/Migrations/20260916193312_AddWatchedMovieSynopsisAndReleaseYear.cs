using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozyCinema.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchedMovieSynopsisAndReleaseYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "WatchedMovies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Synopsis",
                table: "WatchedMovies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "WatchedMovies");

            migrationBuilder.DropColumn(
                name: "Synopsis",
                table: "WatchedMovies");
        }
    }
}
