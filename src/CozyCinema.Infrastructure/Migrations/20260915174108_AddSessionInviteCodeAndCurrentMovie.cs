using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozyCinema.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionInviteCodeAndCurrentMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentMovieCoverUrl",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentMovieTitle",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentMovieTmdbId",
                table: "Sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Sessions",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_InviteCode",
                table: "Sessions",
                column: "InviteCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_InviteCode",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CurrentMovieCoverUrl",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CurrentMovieTitle",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CurrentMovieTmdbId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Sessions");
        }
    }
}
