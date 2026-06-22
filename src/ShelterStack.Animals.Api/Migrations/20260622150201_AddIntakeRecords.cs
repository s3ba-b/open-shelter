using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShelterStack.Animals.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIntakeRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntakeRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IntakeType = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    Notes = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeRecords_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_IntakeRecords_AnimalId",
                table: "IntakeRecords",
                column: "AnimalId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "IntakeRecords");
        }
    }
}
