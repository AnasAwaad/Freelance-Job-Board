using FluentValidation.TestHelper;
using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;

namespace FreelanceJobBoard.Application.Tests.Jobs.Commands.CreateJob;
public class CreateJobCommandValidatorTests
{
	[Fact]
	public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
	{
		// Arrange 
		var command = new CreateJobCommand
		{
			Title = "name",
			Description = "description",
			BudgetMin = 10,
			BudgetMax = 20,
			Deadline = DateTime.Now.AddDays(1),
			SkillIds = [1, 2, 3],
			CategoryIds = [1]
		};

		var validator = new CreateJobCommandValidator();


		// Act

		var result = validator.TestValidate(command);

		// Assert

		result.ShouldNotHaveValidationErrorFor(c => c.Title);
		result.ShouldNotHaveValidationErrorFor(c => c.Description);
		result.ShouldNotHaveValidationErrorFor(c => c.BudgetMin);
		result.ShouldNotHaveValidationErrorFor(c => c.BudgetMax);
		result.ShouldNotHaveValidationErrorFor(c => c.Deadline);
		result.ShouldNotHaveValidationErrorFor(c => c.SkillIds);
		result.ShouldNotHaveValidationErrorFor(c => c.CategoryIds);
	}



	[Fact]

	public void Validator_ForInvalidCommand_ShouldHaveValidationErrors()
	{
		// Arrange
		var command = new CreateJobCommand
		{
			Title = new string('f', 256),
			Description = new string('f', 2001),
			BudgetMin = -1,
			BudgetMax = -1,
			Deadline = DateTime.Now,
			SkillIds = new List<int>(),
			CategoryIds = new List<int>()
		};

		var validator = new CreateJobCommandValidator();
		// Act

		var result = validator.TestValidate(command);

		// Assert
		result.ShouldHaveValidationErrorFor(c => c.Title);
		result.ShouldHaveValidationErrorFor(c => c.Description);
		result.ShouldHaveValidationErrorFor(c => c.BudgetMin);
		result.ShouldHaveValidationErrorFor(c => c.BudgetMax);
		result.ShouldHaveValidationErrorFor(c => c.Deadline);
		result.ShouldHaveValidationErrorFor(c => c.SkillIds);
		result.ShouldHaveValidationErrorFor(c => c.CategoryIds);
	}
}
