using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePath.Migrations.SqliteDb
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MapElement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lat = table.Column<double>(type: "REAL", nullable: false),
                    Lng = table.Column<double>(type: "REAL", nullable: false),
                    OSMNodeId = table.Column<long>(type: "INTEGER", nullable: true),
                    EdgeId = table.Column<uint>(type: "INTEGER", nullable: true),
                    VertexId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ItineroMappingError = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapElement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyScoreElement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Score = table.Column<float>(type: "REAL", nullable: false),
                    EdgeId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyScoreElement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyScoreElementMapElement",
                columns: table => new
                {
                    SafetyScoreElementId = table.Column<int>(type: "INTEGER", nullable: false),
                    MapElementId = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "IX_MapElement_Lat_Lng",
                table: "MapElement",
                columns: new[] { "Lat", "Lng" });

            migrationBuilder.CreateIndex(
                name: "IX_SafetyScoreElement_EdgeId",
                table: "SafetyScoreElement",
                column: "EdgeId",
                unique: true);

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
