using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePath.Migrations.FastStorage
{
    /// <inheritdoc />
    public partial class IndexesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SafetyScoreElement_EdgeId",
                table: "SafetyScoreElement",
                column: "EdgeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapElement_Lat_Lng",
                table: "MapElement",
                columns: new[] { "Lat", "Lng" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SafetyScoreElement_EdgeId",
                table: "SafetyScoreElement");

            migrationBuilder.DropIndex(
                name: "IX_MapElement_Lat_Lng",
                table: "MapElement");
        }
    }
}
