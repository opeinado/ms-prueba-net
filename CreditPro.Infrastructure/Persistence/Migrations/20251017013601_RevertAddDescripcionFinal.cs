using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditPro.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RevertAddDescripcionFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescripcionFinal",
                table: "credit_applications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescripcionFinal",
                table: "credit_applications",
                type: "text",
                nullable: true);
        }
    }
}
