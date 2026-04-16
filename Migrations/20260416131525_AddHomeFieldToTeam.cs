using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeatingChartApp.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeFieldToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeField",
                table: "Teams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: 1,
                column: "HomeField",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeField",
                table: "Teams");
        }
    }
}
