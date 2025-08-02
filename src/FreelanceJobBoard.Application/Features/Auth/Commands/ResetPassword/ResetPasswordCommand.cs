﻿using MediatR;
namespace FreelanceJobBoard.Application.Features.Auth.Commands.ResetPassword;
public class ResetPasswordCommand : IRequest<Unit>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}