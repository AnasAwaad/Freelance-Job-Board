using FluentValidation;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
public class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
	public UpdateJobCommandValidator()
	{
		RuleFor(j => j.Title)
			.NotEmpty().WithMessage("{PropertyName} is required!")
			.MaximumLength(255).WithMessage("{PropertyName} must not exceed {MaxLength} characters.");

		RuleFor(j => j.Description)
			.NotEmpty().WithMessage("{PropertyName} is required!")
			.MaximumLength(2000).WithMessage("{PropertyName} must not exceed {MaxLength} characters.");

		RuleFor(j => j.BudgetMin)
			.GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0.");

		RuleFor(j => j.BudgetMax)
			.GreaterThan(x => x.BudgetMin).WithMessage("{PropertyName} must be greater than BudgetMin.");

		RuleFor(j => j.Deadline)
			.Must(BeAFutureDate).WithMessage("{PropertyName} must be a future date.");

		RuleFor(x => x.SkillIds)
			.NotNull().WithMessage("{PropertyName} are required.")
			.Must(skills => skills.Any()).WithMessage("At least one skill must be selected.");

		RuleFor(x => x.CategoryIds)
			.NotNull().WithMessage("{PropertyName} are required.")
			.Must(cats => cats.Any()).WithMessage("At least one category must be selected.");


	}

	private bool BeAFutureDate(DateTime date) =>
		date > DateTime.Now;
}
