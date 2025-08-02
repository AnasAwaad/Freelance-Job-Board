using FluentValidation.TestHelper;
using FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;

namespace FreelanceJobBoard.Application.Tests.Categories.Commands.CreateCategory;
public class CreateCategoryCommandValidatorTests
{

	[Fact]
	public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
	{
		// Arrange 
		var command = new CreateCategoryCommand
		{
			Name = "name",
			Description = "description",
		};

		var validator = new CreateCategoryCommandValidator();


		// Act

		var result = validator.TestValidate(command);

		// Assert

		result.ShouldNotHaveValidationErrorFor(c => c.Name);
		result.ShouldNotHaveValidationErrorFor(c => c.Description);
	}

	[Fact]

	public void Validator_ForInvalidCommand_ShouldHaveValidationErrors()
	{
		// Arrange
		var command = new CreateCategoryCommand();
		var validator = new CreateCategoryCommandValidator();
		// Act

		var retult = validator.TestValidate(command);

		// Assert

		retult.ShouldHaveValidationErrorFor(c => c.Name);
		retult.ShouldHaveValidationErrorFor(c => c.Description);
	}

	[Fact]
	public void Validator_ForTooLongName_ShouldHaveValidationError()
	{
		// Arrange
		var command = new CreateCategoryCommand()
		{
			Name = new string('t', 201),
			Description = "description",
		};
		var validator = new CreateCategoryCommandValidator();
		// Act

		var retult = validator.TestValidate(command);

		// Assert

		retult.ShouldHaveValidationErrorFor(c => c.Name)
			.WithErrorMessage("Name must not exceed 200 characters.");
	}

	[Fact]
	public void Validator_ForTooLongDescription_ShouldHaveValidationError()
	{
		// Arrange
		var command = new CreateCategoryCommand()
		{
			Name = "test",
			Description = new string('t', 1001),
		};
		var validator = new CreateCategoryCommandValidator();
		// Act

		var retult = validator.TestValidate(command);

		// Assert

		retult.ShouldHaveValidationErrorFor(c => c.Description)
			.WithErrorMessage("Description must not exceed 1000 characters.");
	}
}
