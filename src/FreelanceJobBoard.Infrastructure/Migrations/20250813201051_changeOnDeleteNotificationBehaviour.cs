using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeOnDeleteNotificationBehaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Jobs_JobId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Proposals_ProposalId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Jobs_JobId",
                table: "Notifications",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Proposals_ProposalId",
                table: "Notifications",
                column: "ProposalId",
                principalTable: "Proposals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Jobs_JobId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Proposals_ProposalId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Jobs_JobId",
                table: "Notifications",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Proposals_ProposalId",
                table: "Notifications",
                column: "ProposalId",
                principalTable: "Proposals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
