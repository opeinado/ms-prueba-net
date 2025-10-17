using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditPro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultValueToDescripcionFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "collateral_description",
                table: "credit_applications",
                newName: "CollateralDescription");

            migrationBuilder.AddColumn<string>(
                name: "DescripcionFinal",
                table: "credit_applications",
                type: "text",
                nullable: false,
                defaultValue: "hola soy descripcion final");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescripcionFinal",
                table: "credit_applications");

            migrationBuilder.RenameColumn(
                name: "CollateralDescription",
                table: "credit_applications",
                newName: "collateral_description");
        }
    }
}
