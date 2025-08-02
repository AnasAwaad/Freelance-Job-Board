using FluentValidation;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.FreelancerRegister;
public class FreelancerRegisterCommandValidator : AbstractValidator<FreelancerRegisterCommand>
{
    public FreelancerRegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.");
    }
}