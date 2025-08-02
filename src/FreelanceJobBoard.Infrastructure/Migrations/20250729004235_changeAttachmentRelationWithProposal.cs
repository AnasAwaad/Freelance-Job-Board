using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeAttachmentRelationWithProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposalAttachments_Proposals_ProposalId",
                table: "ProposalAttachments");

            migrationBuilder.AddForeignKey(
                name: "FK_ProposalAttachments_Proposals_ProposalId",
                table: "ProposalAttachments",
                column: "ProposalId",
                principalTable: "Proposals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposalAttachments_Proposals_ProposalId",
                table: "ProposalAttachments");

            migrationBuilder.AddForeignKey(
                name: "FK_ProposalAttachments_Proposals_ProposalId",
                table: "ProposalAttachments",
                column: "ProposalId",
                principalTable: "Proposals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
