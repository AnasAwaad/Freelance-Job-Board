using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
public class ClientRegisterCommand : IRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    //public string? ProfileImageUrl { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyDescription { get; set; }
    //public string? CompanyLogoUrl { get; set; }
    //public string? CompanyWebsiteUrl { get; set; }
    public string? CompanyIndustry { get; set; }
}
