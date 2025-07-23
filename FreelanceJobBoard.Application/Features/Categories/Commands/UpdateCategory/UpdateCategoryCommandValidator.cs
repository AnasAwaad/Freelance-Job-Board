using FluentValidation;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
	public UpdateCategoryCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("{PropertyName} is required.")
			.MaximumLength(200)
			.WithMessage("{PropertyName} must not exceed {MaxLength} characters.");

		RuleFor(x => x.Description)
			.NotEmpty()
			.WithMessage("{PropertyName} is required.")
			.MaximumLength(1000)
			.WithMessage("{PropertyName} must not exceed {MaxLength} characters.");
	}
}
