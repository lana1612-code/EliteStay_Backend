using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel_Backend_API.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusForPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusDone",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusDone",
                table: "Payments");
        }
    }
}
