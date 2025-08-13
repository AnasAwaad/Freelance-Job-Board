using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Infrastructure.Settings;

public class EmailSettings
{
    [Required(ErrorMessage = "SMTP Server is required")]
    public string SmtpServer { get; set; } = null!;

    [Range(1, 65535, ErrorMessage = "SMTP Port must be between 1 and 65535")]
    public int SmtpPort { get; set; }

    [Required(ErrorMessage = "From Email is required")]
    [EmailAddress(ErrorMessage = "From Email must be a valid email address")]
    public string FromEmail { get; set; } = null!;

    [Required(ErrorMessage = "From Name is required")]
    public string FromName { get; set; } = null!;

    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public bool EnableSsl { get; set; } = true;
    public bool UseCredentials { get; set; } = true;

    /// <summary>
    /// Timeout in milliseconds for SMTP operations. Default is 30000 (30 seconds).
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 30000;

    /// <summary>
    /// Maximum number of retry attempts for failed email sends. Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in milliseconds. Default is 1000 (1 second).
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Enable/disable email sending globally. Useful for testing environments.
    /// </summary>
    public bool EnableEmailSending { get; set; } = true;

    /// <summary>
    /// When email sending is disabled, optionally log email content instead.
    /// </summary>
    public bool LogEmailsWhenDisabled { get; set; } = true;

    /// <summary>
    /// Default encoding for email content. Default is UTF-8.
    /// </summary>
    public string DefaultEncoding { get; set; } = "UTF-8";

    /// <summary>
    /// Validates the email settings configuration
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(SmtpServer) || string.IsNullOrEmpty(FromEmail) || string.IsNullOrEmpty(FromName))
            return false;

        if (SmtpPort <= 0 || SmtpPort > 65535)
            return false;

        if (UseCredentials && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
            return false;

        // Basic email format validation
        try
        {
            var addr = new System.Net.Mail.MailAddress(FromEmail);
            return addr.Address == FromEmail;
        }
        catch
        {
            return false;
        }
    }
}