using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParqueWeather : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Parques",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Parques",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExterior",
                table: "Parques",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExterior",
                table: "Parques");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Parques",
                type: "decimal(9,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Parques",
                type: "decimal(9,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)");
        }
    }
}
