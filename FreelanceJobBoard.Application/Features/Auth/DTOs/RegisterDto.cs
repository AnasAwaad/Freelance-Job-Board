namespace FreelanceJobBoard.Application.Features.Auth.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Client", "Freelancer", "Admin"
        public string? ProfileImageUrl { get; set; }

        // Client-specific fields


        // Freelancer-specific fields
        public string? Bio { get; set; }
        public string? Description { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? AvailabilityStatus { get; set; } = "Available";
    }
}