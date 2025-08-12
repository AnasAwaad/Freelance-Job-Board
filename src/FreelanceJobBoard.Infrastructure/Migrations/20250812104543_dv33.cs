using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dv33 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionRequestedAt",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionRequestedByUserId",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionRequestedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CompletionRequestedByUserId",
                table: "Contracts");
        }
    }
}
