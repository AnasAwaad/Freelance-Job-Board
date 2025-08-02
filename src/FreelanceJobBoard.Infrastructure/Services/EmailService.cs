using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FreelanceJobBoard.Infrastructure.Services;

public class EmailService : IEmailService
{
	private readonly EmailSettings _emailSettings;
	private readonly ILogger<EmailService> _logger;

	public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
	{
		_emailSettings = emailSettings.Value;
		_logger = logger;
	}

	public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
	{
		await SendEmailAsync(new[] { toEmail }, subject, body, isHtml);
	}

	public async Task SendEmailAsync(IEnumerable<string> toEmails, string subject, string body, bool isHtml = true)
	{
		try
		{
			using var client = CreateSmtpClient();
			using var message = new MailMessage();

			message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);

			foreach (var email in toEmails)
			{
				if (IsValidEmail(email))
				{
					message.To.Add(email);
				}
			}

			if (message.To.Count == 0)
			{
				_logger.LogWarning("No valid email addresses provided");
				return;
			}

			message.Subject = subject;
			message.Body = body;
			message.IsBodyHtml = isHtml;

			await client.SendMailAsync(message);

			_logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", toEmails));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {Recipients}. Subject: {Subject}",
				string.Join(", ", toEmails), subject);
			throw;
		}
	}

	public async Task SendTemplateEmailAsync(string toEmail, string templateName, object templateData)
	{
		var body = ProcessTemplate(templateName, templateData);
		var subject = ExtractSubjectFromTemplate(templateName);

		await SendEmailAsync(toEmail, subject, body, true);
	}

	public async Task SendJobUpdateNotificationAsync(string freelancerEmail, string jobTitle, string status, string? clientMessage = null)
	{
		try
		{
			var subject = $"Update on your job application: {jobTitle}";
			var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Job Application Update</h2>
    <p>Hello,</p>
    <p>We have an update regarding your application for the job: <strong>{jobTitle}</strong></p>
    <p>Status: <strong>{status}</strong></p>
    {(!string.IsNullOrEmpty(clientMessage) ? $"<p>Message from Client: {clientMessage}</p>" : "")}
    <p>Best regards,<br>FreelanceJobBoard Team</p>
</body>
</html>";

			await SendEmailAsync(freelancerEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send job update notification to {Email}", freelancerEmail);
			throw;
		}
	}

	public async Task SendNewProposalNotificationAsync(string clientEmail, string jobTitle, string freelancerName, decimal bidAmount)
	{
		try
		{
			var subject = $"New proposal received for: {jobTitle}";
			var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>New Proposal Received!</h2>
    <p>Hello,</p>
    <p>Great news! You've received a new proposal for your job: <strong>{jobTitle}</strong></p>
    <p><strong>Freelancer:</strong> {freelancerName}</p>
    <p><strong>Bid Amount:</strong> ${bidAmount:N2}</p>
    <p>Review the full proposal in your dashboard.</p>
    <p>Best regards,<br>FreelanceJobBoard Team</p>
</body>
</html>";

			await SendEmailAsync(clientEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send new proposal notification to {Email}", clientEmail);
			throw;
		}
	}

	public async Task SendJobApprovalNotificationAsync(string clientEmail, string jobTitle, bool isApproved, string? adminMessage = null)
	{
		try
		{
			var status = isApproved ? "Approved" : "Rejected";
			var subject = $"Job {status}: {jobTitle}";
			var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Job {status}</h2>
    <p>Hello,</p>
    <p>Your job posting has been <strong>{status.ToLower()}</strong> by our admin team: <strong>{jobTitle}</strong></p>
    {(!string.IsNullOrEmpty(adminMessage) ? $"<p>Admin Message: {adminMessage}</p>" : "")}
    {(isApproved ? "<p>Your job is now live and visible to freelancers!</p>" : "<p>Please review the feedback and make necessary changes.</p>")}
    <p>Best regards,<br>FreelanceJobBoard Admin Team</p>
</body>
</html>";

			await SendEmailAsync(clientEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send job approval notification to {Email}", clientEmail);
			throw;
		}
	}

	private SmtpClient CreateSmtpClient()
	{
		var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
		{
			EnableSsl = _emailSettings.EnableSsl
		};

		if (_emailSettings.UseCredentials)
		{
			client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
		}

		return client;
	}

	private static bool IsValidEmail(string email)
	{
		try
		{
			var addr = new MailAddress(email);
			return addr.Address == email;
		}
		catch
		{
			return false;
		}
	}

	private static string ProcessTemplate(string templateName, object data)
	{
		// Simple template processing - can be enhanced with a proper template engine
		var template = GetTemplate(templateName);
		var result = template;
		var properties = data.GetType().GetProperties();

		foreach (var prop in properties)
		{
			var value = prop.GetValue(data)?.ToString() ?? "";
			result = result.Replace($"{{{{{prop.Name}}}}}", value);
		}

		return result;
	}

	private static string GetTemplate(string templateName)
	{
		return templateName switch
		{
			"JobStatusUpdate" => "<html><body><h2>Job Status Update</h2><p>{{Message}}</p></body></html>",
			"NewProposal" => "<html><body><h2>New Proposal</h2><p>{{Message}}</p></body></html>",
			"JobApproval" => "<html><body><h2>Job Approval</h2><p>{{Message}}</p></body></html>",
			_ => "<html><body><p>{{Message}}</p></body></html>"
		};
	}

	private static string ExtractSubjectFromTemplate(string templateName)
	{
		return templateName switch
		{
			"JobStatusUpdate" => "Job Application Status Update",
			"NewProposal" => "New Proposal Received",
			"JobApproval" => "Job Approval Status",
			_ => "Notification from FreelanceJobBoard"
		};
	}
}