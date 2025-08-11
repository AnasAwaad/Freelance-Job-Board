using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration has been cleaned up - contract attachments removed
            migrationBuilder.DropTable(
                name: "ContractAttachments",
                schema: "dbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration has been cleaned up - contract attachments removed
            migrationBuilder.CreateTable(
                name: "ContractAttachments",
                schema: "dbo",
                columns: table => new
                {
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    AttachmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAttachments", x => new { x.ContractId, x.AttachmentId });
                });
        }
    }
}
