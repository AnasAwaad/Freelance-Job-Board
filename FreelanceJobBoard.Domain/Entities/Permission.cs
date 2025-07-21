using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;

public class Permission : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty; 
    public string Action { get; set; } = string.Empty;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}