using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreelanceJobBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultNotificationTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert a default notification template
            migrationBuilder.Sql(@"
                INSERT INTO NotificationTemplates (TemplateName, TemplateTitle, TemplateMessage, IsActive, CreatedOn)
                VALUES 
                ('General', 'General Notification', 'General notification message', 1, GETUTCDATE())
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the default notification template
            migrationBuilder.Sql(@"
                DELETE FROM NotificationTemplates 
                WHERE TemplateName = 'General' AND TemplateTitle = 'General Notification'
            ");
        }
    }
}