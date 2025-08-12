using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Pending Approval");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Completed");

            migrationBuilder.InsertData(
                table: "ContractStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 5, "Cancelled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Completed");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Cancelled");
        }
    }
}
