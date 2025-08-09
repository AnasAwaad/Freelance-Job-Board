using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FreelanceJobBoard.Infrastructure.Services;

public class EmailSender : IEmailSender
{
	private readonly EmailSetting _mailSettings;
	private readonly IWebHostEnvironment _webHostEnvironment;

	public EmailSender(IOptions<EmailSetting> mailSettings, IWebHostEnvironment webHostEnvironment)
	{
		_mailSettings = mailSettings.Value;
		_webHostEnvironment = webHostEnvironment;
	}

	public async Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		var message = new MailMessage()
		{
			From = new MailAddress(_webHostEnvironment.IsDevelopment() ? "anas.shaban.awaad@gmail.com" : _mailSettings.Email, _mailSettings.DisplayName),
			Subject = subject,
			Body = htmlMessage,
			IsBodyHtml = true,
		};

		message.To.Add(email);

		SmtpClient smtpClient = new SmtpClient(_mailSettings.Host)
		{
			Port = _mailSettings.Port,
			Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password),
			EnableSsl = true
		};
		await smtpClient.SendMailAsync(message);
		smtpClient.Dispose();

	}
}