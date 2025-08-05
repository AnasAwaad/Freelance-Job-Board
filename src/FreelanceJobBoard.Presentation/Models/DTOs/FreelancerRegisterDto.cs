namespace FreelanceJobBoard.Presentation.Models.DTOs;

public class FreelancerRegisterDto
{
	public string Email { get; set; } = null!;
	public string Password { get; set; } = null!;
	public string FullName { get; set; } = null!;
}
