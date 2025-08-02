namespace FreelanceJobBoard.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    Task SendEmailAsync(IEnumerable<string> toEmails, string subject, string body, bool isHtml = true);
    Task SendTemplateEmailAsync(string toEmail, string templateName, object templateData);
    Task SendJobUpdateNotificationAsync(string freelancerEmail, string jobTitle, string status, string? clientMessage = null);
    Task SendNewProposalNotificationAsync(string clientEmail, string jobTitle, string freelancerName, decimal bidAmount);
    Task SendJobApprovalNotificationAsync(string clientEmail, string jobTitle, bool isApproved, string? adminMessage = null);
}