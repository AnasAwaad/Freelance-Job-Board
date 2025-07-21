using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedBaseEntityFromJobAttachmentProposalAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "ProposalAttachments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProposalAttachments");

            migrationBuilder.DropColumn(
                name: "LastUpdatedOn",
                table: "ProposalAttachments");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "JobAttachments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobAttachments");

            migrationBuilder.DropColumn(
                name: "LastUpdatedOn",
                table: "JobAttachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "ProposalAttachments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProposalAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedOn",
                table: "ProposalAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "JobAttachments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JobAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedOn",
                table: "JobAttachments",
                type: "datetime2",
                nullable: true);
        }
    }
}
