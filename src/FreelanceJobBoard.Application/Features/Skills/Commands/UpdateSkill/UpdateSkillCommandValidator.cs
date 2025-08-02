using FluentValidation;

namespace FreelanceJobBoard.Application.Features.Skills.Commands.UpdateSkill;

public class UpdateSkillCommandValidator : AbstractValidator<UpdateSkillCommand>
{
    public UpdateSkillCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Skill ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Skill name is required.")
            .MaximumLength(255)
            .WithMessage("Skill name must not exceed 255 characters.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Skill name cannot be empty or whitespace.");
    }
}