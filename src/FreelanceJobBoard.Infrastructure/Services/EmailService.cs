using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

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
		// Check if email sending is enabled
		if (!_emailSettings.EnableEmailSending)
		{
			if (_emailSettings.LogEmailsWhenDisabled)
			{
				_logger.LogInformation("Email sending disabled. Would send email - To: {Recipients}, Subject: {Subject}",
					string.Join(", ", toEmails), subject);
			}
			return;
		}

		// Validate email settings
		ValidateEmailSettings();

		var retryCount = 0;
		var maxRetries = Math.Max(1, _emailSettings.MaxRetryAttempts);

		while (retryCount < maxRetries)
		{
			try
			{
				await SendEmailInternalAsync(toEmails, subject, body, isHtml);
				_logger.LogInformation("Email sent successfully to {Recipients} on attempt {Attempt}", 
					string.Join(", ", toEmails), retryCount + 1);
				return;
			}
			catch (Exception ex) when (retryCount < maxRetries - 1)
			{
				retryCount++;
				_logger.LogWarning(ex, "Failed to send email to {Recipients} on attempt {Attempt}. Retrying in {Delay}ms...",
					string.Join(", ", toEmails), retryCount, _emailSettings.RetryDelayMilliseconds);
				
				await Task.Delay(_emailSettings.RetryDelayMilliseconds);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send email to {Recipients} after {MaxRetries} attempts. Subject: {Subject}",
					string.Join(", ", toEmails), maxRetries, subject);
				throw;
			}
		}
	}

	private async Task SendEmailInternalAsync(IEnumerable<string> toEmails, string subject, string body, bool isHtml)
	{
		using var client = CreateSmtpClient();
		using var message = CreateMailMessage(toEmails, subject, body, isHtml);

		if (message.To.Count == 0)
		{
			_logger.LogWarning("No valid email addresses provided");
			return;
		}

		await client.SendMailAsync(message);
	}

	private MailMessage CreateMailMessage(IEnumerable<string> toEmails, string subject, string body, bool isHtml)
	{
		var message = new MailMessage();
		
		message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);

		foreach (var email in toEmails)
		{
			if (IsValidEmail(email))
			{
				message.To.Add(email);
			}
			else
			{
				_logger.LogWarning("Invalid email address skipped: {Email}", email);
			}
		}

		message.Subject = subject;
		message.Body = body;
		message.IsBodyHtml = isHtml;
		
		// Set encoding
		var encoding = GetEncoding(_emailSettings.DefaultEncoding);
		message.SubjectEncoding = encoding;
		message.BodyEncoding = encoding;

		return message;
	}

	public async Task SendTemplateEmailAsync(string toEmail, string templateName, object templateData)
	{
		try
		{
			var body = ProcessTemplate(templateName, templateData);
			var subject = ExtractSubjectFromTemplate(templateName);

			await SendEmailAsync(toEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send template email {TemplateName} to {Email}", templateName, toEmail);
			throw;
		}
	}

	public async Task SendJobUpdateNotificationAsync(string freelancerEmail, string jobTitle, string status, string? clientMessage = null)
	{
		try
		{
			var subject = $"Update on your job application: {jobTitle}";
			var body = GenerateJobUpdateEmail(jobTitle, status, clientMessage);

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
			var body = GenerateNewProposalEmail(jobTitle, freelancerName, bidAmount);

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
			var body = GenerateJobApprovalEmail(jobTitle, isApproved, adminMessage);

			await SendEmailAsync(clientEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send job approval notification to {Email}", clientEmail);
			throw;
		}
	}

	public async Task SendJobSubmissionNotificationAsync(string adminEmail, string jobTitle, string clientName, decimal budgetMin, decimal budgetMax)
	{
		try
		{
			var subject = $"New Job Submission Requires Approval: {jobTitle}";
			var body = GenerateJobSubmissionEmail(jobTitle, clientName, budgetMin, budgetMax);

			await SendEmailAsync(adminEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send job submission notification to {Email}", adminEmail);
			throw;
		}
	}

	public async Task SendWelcomeEmailAsync(string userEmail, string userName, string userRole)
	{
		try
		{
			var subject = "Welcome to FreelanceJobBoard!";
			var body = GenerateWelcomeEmail(userName, userEmail, userRole);

			await SendEmailAsync(userEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send welcome email to {Email}", userEmail);
			throw;
		}
	}

	public async Task SendContractStatusNotificationAsync(string userEmail, string contractTitle, string newStatus, string counterpartyName)
	{
		try
		{
			var subject = $"Contract Status Update: {contractTitle}";
			var body = GenerateContractStatusEmail(contractTitle, newStatus, counterpartyName);

			await SendEmailAsync(userEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send contract status notification to {Email}", userEmail);
			throw;
		}
	}

	public async Task SendReviewNotificationAsync(string userEmail, string revieweeId, string reviewerName, string jobTitle, int rating)
	{
		try
		{
			var subject = $"New Review Received: {jobTitle}";
			var body = GenerateReviewNotificationEmail(reviewerName, jobTitle, rating);

			await SendEmailAsync(userEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send review notification to {Email}", userEmail);
			throw;
		}
	}

	public async Task SendPaymentNotificationAsync(string userEmail, decimal amount, string jobTitle, string transactionType = "received")
	{
		try
		{
			var subject = $"Payment {transactionType}: ${amount:N2}";
			var body = GeneratePaymentNotificationEmail(amount, jobTitle, transactionType);

			await SendEmailAsync(userEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send payment notification to {Email}", userEmail);
			throw;
		}
	}

	public async Task SendDeadlineReminderAsync(string userEmail, string itemName, string itemType, DateTime deadline, int daysRemaining)
	{
		try
		{
			var subject = $"Deadline Reminder: {itemName}";
			var body = GenerateDeadlineReminderEmail(itemName, itemType, deadline, daysRemaining);

			await SendEmailAsync(userEmail, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send deadline reminder to {Email}", userEmail);
			throw;
		}
	}

	#region Private Helper Methods

	private void ValidateEmailSettings()
	{
		if (!_emailSettings.IsValid())
		{
			throw new InvalidOperationException("Email settings are not properly configured. Please check your email configuration.");
		}
	}

	private SmtpClient CreateSmtpClient()
	{
		var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
		{
			EnableSsl = _emailSettings.EnableSsl,
			Timeout = _emailSettings.TimeoutMilliseconds
		};

		if (_emailSettings.UseCredentials)
		{
			client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
		}

		return client;
	}

	private static bool IsValidEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
			return false;

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

	private static Encoding GetEncoding(string encodingName)
	{
		try
		{
			return Encoding.GetEncoding(encodingName);
		}
		catch
		{
			return Encoding.UTF8; // Fallback to UTF-8
		}
	}

	#endregion

	#region Email Template Generators

	private static string GenerateJobUpdateEmail(string jobTitle, string status, string? clientMessage)
	{
		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #007bff;'>Job Application Update</h2>
        <p>Hello,</p>
        <p>We have an update regarding your application for the job: <strong>{jobTitle}</strong></p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #007bff;'>
            <p><strong>Status:</strong> {status}</p>
            {(!string.IsNullOrEmpty(clientMessage) ? $"<p><strong>Message from Client:</strong> {clientMessage}</p>" : "")}
        </div>
        <p>Best regards,<br><strong>FreelanceJobBoard Team</strong></p>
    </div>
</body>
</html>";
	}

	private static string GenerateNewProposalEmail(string jobTitle, string freelancerName, decimal bidAmount)
	{
		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #28a745;'>New Proposal Received!</h2>
        <p>Hello,</p>
        <p>Great news! You've received a new proposal for your job: <strong>{jobTitle}</strong></p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745;'>
            <p><strong>Freelancer:</strong> {freelancerName}</p>
            <p><strong>Bid Amount:</strong> ${bidAmount:N2}</p>
        </div>
        <p>Review the full proposal in your dashboard.</p>
        <p>Best regards,<br><strong>FreelanceJobBoard Team</strong></p>
    </div>
</body>
</html>";
	}

	private static string GenerateJobApprovalEmail(string jobTitle, bool isApproved, string? adminMessage)
	{
		var status = isApproved ? "Approved" : "Rejected";
		var color = isApproved ? "#28a745" : "#dc3545";
		var statusMessage = isApproved ? 
			"Your job is now live and visible to freelancers!" : 
			"Please review the feedback and make necessary changes.";

		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: {color};'>Job {status}</h2>
        <p>Hello,</p>
        <p>Your job posting has been <strong>{status.ToLower()}</strong> by our admin team: <strong>{jobTitle}</strong></p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid {color};'>
            {(!string.IsNullOrEmpty(adminMessage) ? $"<p><strong>Admin Message:</strong> {adminMessage}</p>" : "")}
            <p><em>{statusMessage}</em></p>
        </div>
        <p>Best regards,<br><strong>FreelanceJobBoard Admin Team</strong></p>
    </div>
</body>
</html>";
	}

	private static string GenerateJobSubmissionEmail(string jobTitle, string clientName, decimal budgetMin, decimal budgetMax)
	{
		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #ffc107;'>New Job Submission</h2>
        <p>Hello Admin,</p>
        <p>A new job posting has been submitted and requires your approval:</p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107;'>
            <h3 style='margin-top: 0;'>{jobTitle}</h3>
            <p><strong>Client:</strong> {clientName}</p>
            <p><strong>Budget:</strong> ${budgetMin:N2} - ${budgetMax:N2}</p>
        </div>
        <p>Please review and approve or reject this job posting in the admin panel.</p>
        <p>Best regards,<br><strong>FreelanceJobBoard System</strong></p>
    </div>
</body>
</html>";
	}

	private static string GenerateWelcomeEmail(string userName, string userEmail, string userRole)
	{
		var roleSpecificContent = userRole.ToLower() == "client" ?
			"<li>Post your first job to find talented freelancers</li><li>Browse freelancer profiles and portfolios</li>" :
			"<li>Browse available jobs and submit proposals</li><li>Showcase your skills and build your portfolio</li>";

		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #007bff;'>Welcome to FreelanceJobBoard!</h1>
        <p>Hello {userName},</p>
        <p>Welcome to our platform! We're excited to have you join our community as a <strong>{userRole}</strong>.</p>
        
        <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #007bff;'>
            <h3 style='margin-top: 0; color: #007bff;'>Get Started:</h3>
            <ul>
                <li>Complete your profile to stand out</li>
                {roleSpecificContent}
                <li>Connect with other professionals</li>
                <li>Build your reputation through reviews</li>
            </ul>
        </div>
        
        <p>If you have any questions or need assistance, our support team is here to help!</p>
        
        <p>Best regards,<br><strong>The FreelanceJobBoard Team</strong></p>
        
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
        <p style='font-size: 12px; color: #666;'>
            This email was sent to {userEmail}. If you didn't create an account with us, please ignore this email.
        </p>
    </div>
</body>
</html>";
	}

	private static string GenerateContractStatusEmail(string contractTitle, string newStatus, string counterpartyName)
	{
		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #007bff;'>Contract Status Update</h2>
        <p>Hello,</p>
        <p>Your contract with <strong>{counterpartyName}</strong> has been updated:</p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #007bff;'>
            <h3 style='margin-top: 0;'>{contractTitle}</h3>
            <p><strong>New Status:</strong> {newStatus}</p>
        </div>
        <p>Please check your dashboard for more details.</p>
        <p>Best regards,<br><strong>FreelanceJobBoard Team</strong></p>
    </div>
</body>
</html>";
	}

	private static string GenerateReviewNotificationEmail(string reviewerName, string jobTitle, int rating)
	{
		// Fix the star display - use actual star characters
		var stars = new string('?', rating) + new string('?', 5 - rating);

		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #28a745;'>New Review Received!</h2>
        <p>Hello,</p>
        <p>Great news! You've received a new review for your work on <strong>{jobTitle}</strong>:</p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745;'>
            <p><strong>Reviewer:</strong> {reviewerName}</p>
            <p><strong>Rating:</strong> {stars} ({rating}/5 stars)</p>
            <p><strong>Project:</strong> {jobTitle}</p>
        </div>
        <p>Reviews help build your reputation on the platform. Keep up the great work!</p>
        <p>Best regards,<br><strong>FreelanceJobBoard Team</strong></p>
    </div>
</body>
</html>";
	}

	private static string GeneratePaymentNotificationEmail(decimal amount, string jobTitle, string transactionType)
	{
		var color = transactionType.ToLower() == "received" ? "#28a745" : "#007bff";
		
		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: {color};'>Payment {transactionType.ToUpper()}</h2>
        <p>Hello,</p>
        <p>A payment has been {transactionType} for your work:</p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid {color};'>
            <p><strong>Amount:</strong> ${amount:N2}</p>
            <p><strong>Project:</strong> {jobTitle}</p>
            <p><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
        </div>
        <p>You can view the payment details in your dashboard.</p>
        <p>Best regards,<br><strong>FreelanceJobBoard Team</strong></p>
    </div>
</body>
</html>";
	}

	private static string GenerateDeadlineReminderEmail(string itemName, string itemType, DateTime deadline, int daysRemaining)
	{
		var urgencyLevel = daysRemaining <= 1 ? "urgent" : daysRemaining <= 3 ? "important" : "reminder";
		var urgencyColor = daysRemaining <= 1 ? "#dc3545" : daysRemaining <= 3 ? "#ffc107" : "#007bff";
		var urgencyText = daysRemaining > 0 ? $"Due in {daysRemaining} day{(daysRemaining == 1 ? "" : "s")}" : "Due today!";
		
		return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: {urgencyColor};'>Deadline Reminder</h2>
        <p>Hello,</p>
        <p>This is a <strong>{urgencyLevel}</strong> reminder about an upcoming deadline:</p>
        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid {urgencyColor};'>
            <h3 style='margin-top: 0;'>{itemName}</h3>
            <p><strong>Type:</strong> {itemType}</p>
            <p><strong>Deadline:</strong> {deadline:yyyy-MM-dd HH:mm} UTC</p>
            <p style='color: {urgencyColor}; font-weight: bold; font-size: 16px;'>
                {urgencyText}
            </p>
        </div>
        <p>Please make sure to complete this on time to maintain your reputation.</p>
        <p>Best regards,<br><strong>FreelanceJobBoard Team</strong></p>
    </div>
</body>
</html>";
	}

	#endregion

	#region Template Processing (Legacy Support)

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
			"Welcome" => "<html><body><h2>Welcome!</h2><p>{{Message}}</p></body></html>",
			"ContractUpdate" => "<html><body><h2>Contract Update</h2><p>{{Message}}</p></body></html>",
			"PaymentNotification" => "<html><body><h2>Payment Notification</h2><p>{{Message}}</p></body></html>",
			"ReviewNotification" => "<html><body><h2>Review Notification</h2><p>{{Message}}</p></body></html>",
			"DeadlineReminder" => "<html><body><h2>Deadline Reminder</h2><p>{{Message}}</p></body></html>",
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
			"Welcome" => "Welcome to FreelanceJobBoard",
			"ContractUpdate" => "Contract Update",
			"PaymentNotification" => "Payment Notification",
			"ReviewNotification" => "Review Notification",
			"DeadlineReminder" => "Deadline Reminder",
			_ => "Notification from FreelanceJobBoard"
		};
	}

	#endregion
}