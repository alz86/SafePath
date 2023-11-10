using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePath.Migrations.FastStorage
{
    /// <inheritdoc />
    public partial class InitialMigrationSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MapElement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Lng = table.Column<double>(type: "float", nullable: false),
                    OSMNodeId = table.Column<long>(type: "bigint", nullable: true),
                    EdgeId = table.Column<long>(type: "bigint", nullable: true),
                    VertexId = table.Column<long>(type: "bigint", nullable: true),
                    ItineroMappingError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapElement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyScoreElement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Score = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyScoreElement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyScoreElementMapElement",
                columns: table => new
                {
                    SafetyScoreElementId = table.Column<int>(type: "int", nullable: false),
                    MapElementId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyScoreElementMapElement", x => new { x.SafetyScoreElementId, x.MapElementId });
                    table.ForeignKey(
                        name: "FK_SafetyScoreElementMapElement_MapElement_MapElementId",
                        column: x => x.MapElementId,
                        principalTable: "MapElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SafetyScoreElementMapElement_SafetyScoreElement_SafetyScoreElementId",
                        column: x => x.SafetyScoreElementId,
                        principalTable: "SafetyScoreElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SafetyScoreElementMapElement_MapElementId",
                table: "SafetyScoreElementMapElement",
                column: "MapElementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SafetyScoreElementMapElement");

            migrationBuilder.DropTable(
                name: "MapElement");

            migrationBuilder.DropTable(
                name: "SafetyScoreElement");
        }
    }
}
