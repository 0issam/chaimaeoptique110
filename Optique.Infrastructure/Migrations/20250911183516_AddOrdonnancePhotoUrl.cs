using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Optique.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdonnancePhotoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Ordonnances",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Ordonnances");
        }
    }
}
