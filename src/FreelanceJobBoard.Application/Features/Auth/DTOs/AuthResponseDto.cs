namespace FreelanceJobBoard.Application.DTOs
{
	public class AuthResponseDto
	{
		public string Token { get; set; }
		public string Email { get; set; }
		public string Role { get; set; }
		public string FullName { get; set; }
		public string ProfileImageUrl { get; set; }
	}
}