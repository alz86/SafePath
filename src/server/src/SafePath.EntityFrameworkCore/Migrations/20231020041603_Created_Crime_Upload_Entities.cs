using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafePath.Migrations
{
    /// <inheritdoc />
    public partial class CreatedCrimeUploadEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrimeDataUploading",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrimeDataUploading", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrimeDataUploadingEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrimeDataUploadingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Longitude = table.Column<float>(type: "real", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrimeDataUploadingEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrimeDataUploadingEntry_CrimeDataUploading_CrimeDataUploadingId",
                        column: x => x.CrimeDataUploadingId,
                        principalTable: "CrimeDataUploading",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrimeDataUploadingEntry_CrimeDataUploadingId",
                table: "CrimeDataUploadingEntry",
                column: "CrimeDataUploadingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrimeDataUploadingEntry");

            migrationBuilder.DropTable(
                name: "CrimeDataUploading");
        }
    }
}
