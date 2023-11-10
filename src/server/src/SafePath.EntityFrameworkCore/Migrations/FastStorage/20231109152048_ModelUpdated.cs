using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePath.Migrations.FastStorage
{
    /// <inheritdoc />
    public partial class ModelUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EdgeId",
                table: "SafetyScoreElement",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "MapElement",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EdgeId",
                table: "SafetyScoreElement");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "MapElement");
        }
    }
}
