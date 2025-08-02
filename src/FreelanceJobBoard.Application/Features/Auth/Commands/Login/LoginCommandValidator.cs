using FluentValidation;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.LoginDto.Email)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .EmailAddress()
            .WithMessage("Valid {PropertyName} is required.");

        RuleFor(x => x.LoginDto.Password)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.");
    }
}