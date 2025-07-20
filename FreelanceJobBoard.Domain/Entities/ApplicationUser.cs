using FreelanceJobBoard.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Domain.Identity;
public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; } = null!;
	public string? ProfileImageUrl { get; set; }
	public ICollection<Notification> Notifications { get; set; }
}
