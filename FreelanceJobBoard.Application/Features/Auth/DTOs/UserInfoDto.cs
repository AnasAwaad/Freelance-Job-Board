using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreelanceJobBoard.Application.Features.Auth.DTOs
{
    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public List<string> Roles { get; set; } = new();
        public int? ClientId { get; set; }
        public int? FreelancerId { get; set; }
    }
}
