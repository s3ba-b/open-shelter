using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShelterStack.Animals.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExpandAnimalDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "Animals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true
            );

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Animals",
                type: "date",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Animals",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Animals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Species",
                table: "Animals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Breed", table: "Animals");

            migrationBuilder.DropColumn(name: "DateOfBirth", table: "Animals");

            migrationBuilder.DropColumn(name: "Description", table: "Animals");

            migrationBuilder.DropColumn(name: "Sex", table: "Animals");

            migrationBuilder.DropColumn(name: "Species", table: "Animals");
        }
    }
}
