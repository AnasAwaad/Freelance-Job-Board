using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;

namespace FreelanceJobBoard.Presentation.Services;

public class AuthService
{
	private readonly HttpClient _httpClient;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private readonly ILogger<AuthService> _logger;
	private readonly IEmailService _emailService;

	public AuthService(HttpClient httpClient, IWebHostEnvironment webHostEnvironment, ILogger<AuthService> logger, IEmailService emailService)
	{
		_httpClient = httpClient;
		_webHostEnvironment = webHostEnvironment;
		_logger = logger;
		_emailService = emailService;
		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/Auth/");
	}

	public async Task<AuthResponseDto?> LoginAsync(LoginViewModel viewModel)
	{
		try
		{
			var user = new LoginDto
			{
				Email = viewModel.Email,
				Password = viewModel.Password,
			};

			var response = await _httpClient.PostAsJsonAsync("login", user);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
			}

			_logger.LogWarning("Login failed for user {Email}. Status: {StatusCode}", viewModel.Email, response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during login for user {Email}", viewModel.Email);
			return null;
		}
	}

	public async Task<bool> RegisterAsync(RegisterViewModel viewModel)
	{
		try
		{
			// Handle Register user based on role
			HttpResponseMessage? response = null;

			if (viewModel.Role.Equals(nameof(AppRoles.Client)))
			{
				var dto = new ClientRegisterDto
				{
					Email = viewModel.Email,
					FullName = viewModel.FullName,
					Password = viewModel.Password,
					ProfilePhotoUrl = "",
					CompanyName = viewModel.CompanyName,
					CompanyWebsite = viewModel.CompanyWebsite,
					Industry = viewModel.Industry
				};

				response = await _httpClient.PostAsJsonAsync("client-register", dto);

			}
			else if (viewModel.Role.Equals(nameof(AppRoles.Freelancer)))
			{
				var dto = new FreelancerRegisterDto
				{
					Email = viewModel.Email,
					FullName = viewModel.FullName,
					Password = viewModel.Password,
					ProfilePhotoUrl = "",
					PhoneNumber = viewModel.PhoneNumber ?? string.Empty,
					Bio = viewModel.Bio ?? string.Empty,
					YearsOfExperience = viewModel.YearsOfExperience ?? 0,
					HourlyRate = viewModel.HourlyRate ?? 0,
					PortfolioUrl = viewModel.PortfolioUrl,
					Specialization = viewModel.Specialization ?? string.Empty
				};

				response = await _httpClient.PostAsJsonAsync("freelancer-register", dto);

			}


			if (response.IsSuccessStatusCode)
			{
				// Send welcome email
				await SendWelcomeEmailAsync(viewModel.Email, viewModel.FullName, viewModel.Role);
				return true;
			}

			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Registration error for user {Email}", viewModel.Email);
			return false;
		}
	}

	public async Task<bool> ForgotPasswordAsync(ForgotPasswordViewModel viewModel)
	{
		try
		{
			var dto = new ForgotPasswordDto
			{
				Email = viewModel.Email
			};

			var response = await _httpClient.PostAsJsonAsync("forgot-password", dto);
			if (response.IsSuccessStatusCode)
			{
				// Send password reset email
				await SendPasswordResetEmailAsync(viewModel.Email);
				_logger.LogInformation("Forgot password request sent for user {Email}", viewModel.Email);
				return true;
			}

			_logger.LogWarning("Forgot password failed for user {Email}. Status: {StatusCode}", viewModel.Email, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during forgot password for user {Email}", viewModel.Email);
			return false;
		}
	}

	public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel viewModel)
	{
		try
		{
			var dto = new ResetPasswordDto
			{
				Email = viewModel.Email,
				Token = viewModel.Token,
				NewPassword = viewModel.NewPassword
			};

			var response = await _httpClient.PostAsJsonAsync("reset-password", dto);
			if (response.IsSuccessStatusCode)
			{
				// Send password change confirmation email
				await SendPasswordChangedConfirmationEmailAsync(viewModel.Email);
				_logger.LogInformation("Password reset successfully for user {Email}", viewModel.Email);
				return true;
			}

			_logger.LogWarning("Password reset failed for user {Email}. Status: {StatusCode}", viewModel.Email, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during password reset for user {Email}", viewModel.Email);
			return false;
		}
	}

	public async Task<bool> ChangePasswordAsync(ChangePasswordViewModel viewModel)
	{
		try
		{
			var dto = new ChangePasswordDto
			{
				CurrentPassword = viewModel.CurrentPassword,
				NewPassword = viewModel.NewPassword
			};

			var response = await _httpClient.PostAsJsonAsync("change-password", dto);
			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Password changed successfully for current user");
				return true;
			}

			_logger.LogWarning("Password change failed. Status: {StatusCode}", response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during password change");
			return false;
		}
	}

	public async Task<bool> ConfirmEmailAsync(string userId, string token)
	{
		try
		{
			var dto = new ConfirmEmailDto
			{
				UserId = userId,
				Token = token
			};

			var response = await _httpClient.PostAsJsonAsync("confirm-email", dto);
			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Email confirmed successfully for user {UserId}", userId);
				return true;
			}

			_logger.LogWarning("Email confirmation failed for user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during email confirmation for user {UserId}", userId);
			return false;
		}
	}

	public async Task<bool> ResendEmailConfirmationAsync(string email)
	{
		try
		{
			var dto = new { Email = email };
			var response = await _httpClient.PostAsJsonAsync("resend-email-confirmation", dto);

			if (response.IsSuccessStatusCode)
			{
				// Send email confirmation reminder
				await SendEmailConfirmationReminderAsync(email);
				_logger.LogInformation("Email confirmation resent for user {Email}", email);
				return true;
			}

			_logger.LogWarning("Resend email confirmation failed for user {Email}. Status: {StatusCode}", email, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while resending email confirmation for user {Email}", email);
			return false;
		}
	}

	private async Task SendWelcomeEmailAsync(string email, string fullName, string role)
	{
		try
		{
			var subject = "Welcome to FreelanceJobBoard!";
			var body = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #4f46e5;'>Welcome to FreelanceJobBoard, {fullName}!</h2>
        <p>Thank you for joining our platform as a <strong>{role}</strong>.</p>
        
        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h3>What's next?</h3>
            <ul>
                {(role.ToLower() == "client"
					? "<li>Complete your company profile</li><li>Post your first job</li><li>Browse talented freelancers</li>"
					: "<li>Complete your freelancer profile</li><li>Browse available jobs</li><li>Submit your first proposal</li>")}
            </ul>
        </div>
        
        <p>If you have any questions, feel free to contact our support team.</p>
        
        <p>Best regards,<br>
        <strong>FreelanceJobBoard Team</strong></p>
        
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
        <p style='font-size: 12px; color: #6b7280;'>
            This email was sent to {email}. If you didn't create an account, please ignore this email.
        </p>
    </div>
</body>
</html>";

			await _emailService.SendEmailAsync(email, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send welcome email to {Email}", email);
		}
	}

	private async Task SendPasswordResetEmailAsync(string email)
	{
		try
		{
			var subject = "Password Reset Request - FreelanceJobBoard";
			var body = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #dc2626;'>Password Reset Request</h2>
        <p>We received a request to reset your password for your FreelanceJobBoard account.</p>
        
        <div style='background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0;'>
            <p><strong>Security Notice:</strong> If you didn't request this password reset, please ignore this email and contact our support team immediately.</p>
        </div>
        
        <p>Click the link in your email to reset your password. This link will expire in 24 hours.</p>
        
        <p>Best regards,<br>
        <strong>FreelanceJobBoard Security Team</strong></p>
        
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
        <p style='font-size: 12px; color: #6b7280;'>
            This email was sent to {email} for security purposes.
        </p>
    </div>
</body>
</html>";

			await _emailService.SendEmailAsync(email, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send password reset email to {Email}", email);
		}
	}

	private async Task SendPasswordChangedConfirmationEmailAsync(string email)
	{
		try
		{
			var subject = "Password Successfully Changed - FreelanceJobBoard";
			var body = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #059669;'>Password Successfully Changed</h2>
        <p>Your password has been successfully changed for your FreelanceJobBoard account.</p>
        
        <div style='background-color: #f0fdf4; border-left: 4px solid #059669; padding: 15px; margin: 20px 0;'>
            <p><strong>Security Confirmation:</strong> This change was made on {DateTime.Now:MMMM dd, yyyy} at {DateTime.Now:HH:mm} UTC.</p>
        </div>
        
        <div style='background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0;'>
            <p><strong>Security Alert:</strong> If you didn't make this change, please contact our support team immediately and consider that your account may be compromised.</p>
        </div>
        
        <p>Best regards,<br>
        <strong>FreelanceJobBoard Security Team</strong></p>
        
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
        <p style='font-size: 12px; color: #6b7280;'>
            This email was sent to {email} for security purposes.
        </p>
    </div>
</body>
</html>";

			await _emailService.SendEmailAsync(email, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send password change confirmation email to {Email}", email);
		}
	}

	private async Task SendEmailConfirmationReminderAsync(string email)
	{
		try
		{
			var subject = "Email Confirmation Reminder - FreelanceJobBoard";
			var body = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #4f46e5;'>Confirm Your Email Address</h2>
        <p>Please confirm your email address to complete your FreelanceJobBoard account setup.</p>
        
        <div style='background-color: #eff6ff; border-left: 4px solid #4f46e5; padding: 15px; margin: 20px 0;'>
            <p><strong>Why confirm your email?</strong></p>
            <ul>
                <li>Secure your account</li>
                <li>Receive important notifications</li>
                <li>Access all platform features</li>
            </ul>
        </div>
        
        <p>Click the confirmation link that was sent to this email address to verify your account.</p>
        
        <p>Best regards,<br>
        <strong>FreelanceJobBoard Team</strong></p>
        
        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e5e7eb;'>
        <p style='font-size: 12px; color: #6b7280;'>
            This email was sent to {email}. If you didn't create an account, please ignore this email.
        </p>
    </div>
</body>
</html>";

			await _emailService.SendEmailAsync(email, subject, body, true);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email confirmation reminder to {Email}", email);
		}
	}

	private async Task<string?> SaveProfilePhotoAsync(Microsoft.AspNetCore.Http.IFormFile profilePhoto)
	{
		try
		{
			if (profilePhoto.Length == 0)
				return null;

			// Validate file type
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
			var fileExtension = Path.GetExtension(profilePhoto.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(fileExtension))
				return null;

			// Validate file size (5MB max)
			if (profilePhoto.Length > 5 * 1024 * 1024)
				return null;

			// Create uploads directory if it doesn't exist
			var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
			Directory.CreateDirectory(uploadsFolder);

			// Generate unique filename
			var fileName = $"{Guid.NewGuid()}{fileExtension}";
			var filePath = Path.Combine(uploadsFolder, fileName);

			// Save file
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await profilePhoto.CopyToAsync(fileStream);
			}

			// Return relative path
			return $"/uploads/profiles/{fileName}";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error saving profile photo");
			return null;
		}
	}

	// Keep existing methods for backward compatibility
	public async Task<bool> FreelancerRegisterAsync(FreelancerRegisterViewModel viewModel)
	{
		try
		{
			// Handle profile photo upload
			string? profilePhotoPath = null;
			if (viewModel.ProfilePhoto != null)
			{
				profilePhotoPath = await SaveProfilePhotoAsync(viewModel.ProfilePhoto);
			}

			var user = new FreelancerRegisterDto
			{
				Email = viewModel.Email,
				FullName = viewModel.FullName,
				Password = viewModel.Password,
				ProfilePhotoUrl = "",
				PhoneNumber = viewModel.PhoneNumber,
				Bio = viewModel.Bio,
				YearsOfExperience = viewModel.YearsOfExperience,
				HourlyRate = viewModel.HourlyRate,
				PortfolioUrl = viewModel.PortfolioUrl,
				Specialization = viewModel.Specialization
			};

			var response = await _httpClient.PostAsJsonAsync("freelancer-register", user);
			if (response.IsSuccessStatusCode)
			{
				// Send welcome email
				await SendWelcomeEmailAsync(viewModel.Email, viewModel.FullName, "Freelancer");
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Freelancer registration error for user {Email}", viewModel.Email);
			return false;
		}
	}

	public async Task<bool> ClientRegisterAsync(ClientRegisterViewModel viewModel)
	{
		try
		{
			var user = new ClientRegisterDto
			{
				Email = viewModel.Email,
				FullName = viewModel.FullName,
				Password = viewModel.Password,
			};

			var response = await _httpClient.PostAsJsonAsync("client-register", user);
			if (response.IsSuccessStatusCode)
			{
				// Send welcome email
				await SendWelcomeEmailAsync(viewModel.Email, viewModel.FullName, "Client");
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Client registration error for user {Email}", viewModel.Email);
			return false;
		}
	}
}
