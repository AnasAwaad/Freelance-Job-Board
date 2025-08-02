using FluentValidation;

namespace FreelanceJobBoard.Application.Features.Skills.Commands.CreateSkill;

public class CreateSkillCommandValidator : AbstractValidator<CreateSkillCommand>
{
    public CreateSkillCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Skill name is required.")
            .MaximumLength(255)
            .WithMessage("Skill name must not exceed 255 characters.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Skill name cannot be empty or whitespace.");
    }
}