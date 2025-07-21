using FreelanceJobBoard.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Domain.Entities;

public class RolePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;
    public int PermissionId { get; set; }

    public virtual IdentityRole Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}