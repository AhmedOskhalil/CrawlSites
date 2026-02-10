using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crawl.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Countries_CountryId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CountryId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Departments");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CountryId",
                table: "Employees",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Countries_CountryId",
                table: "Employees",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Countries_CountryId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CountryId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CountryId",
                table: "Departments",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Countries_CountryId",
                table: "Departments",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
