using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePath.Migrations
{
    /// <inheritdoc />
    public partial class NewAreaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Area",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "InitialLatitude",
                table: "Area",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "InitialLongitude",
                table: "Area",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "InitialLatitude",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "InitialLongitude",
                table: "Area");
        }
    }
}
