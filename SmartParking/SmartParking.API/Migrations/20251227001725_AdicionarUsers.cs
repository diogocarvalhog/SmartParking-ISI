using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sensores");

            migrationBuilder.DropTable(
                name: "Lugares");

            migrationBuilder.DropTable(
                name: "Parques");
        }
    }
}
