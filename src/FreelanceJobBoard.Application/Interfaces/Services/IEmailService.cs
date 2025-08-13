namespace FreelanceJobBoard.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    Task SendEmailAsync(IEnumerable<string> toEmails, string subject, string body, bool isHtml = true);
    Task SendTemplateEmailAsync(string toEmail, string templateName, object templateData);
    Task SendJobUpdateNotificationAsync(string freelancerEmail, string jobTitle, string status, string? clientMessage = null);
    Task SendNewProposalNotificationAsync(string clientEmail, string jobTitle, string freelancerName, decimal bidAmount);
    Task SendJobApprovalNotificationAsync(string clientEmail, string jobTitle, bool isApproved, string? adminMessage = null);
    Task SendJobSubmissionNotificationAsync(string adminEmail, string jobTitle, string clientName, decimal budgetMin, decimal budgetMax);
    Task SendWelcomeEmailAsync(string userEmail, string userName, string userRole);
    Task SendContractStatusNotificationAsync(string userEmail, string contractTitle, string newStatus, string counterpartyName);
    Task SendReviewNotificationAsync(string userEmail, string revieweeId, string reviewerName, string jobTitle, int rating);
    Task SendPaymentNotificationAsync(string userEmail, decimal amount, string jobTitle, string transactionType = "received");
    Task SendDeadlineReminderAsync(string userEmail, string itemName, string itemType, DateTime deadline, int daysRemaining);
}