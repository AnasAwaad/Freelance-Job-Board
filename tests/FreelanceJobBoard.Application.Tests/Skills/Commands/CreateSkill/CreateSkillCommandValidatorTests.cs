using FluentValidation.TestHelper;
using FreelanceJobBoard.Application.Features.Skills.Commands.CreateSkill;

namespace FreelanceJobBoard.Application.Tests.Skills.Commands.CreateSkill;

public class CreateSkillCommandValidatorTests
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange 
        var command = new CreateSkillCommand
        {
            Name = "C# Programming"
        };

        var validator = new CreateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_ForEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = ""
        };

        var validator = new CreateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Skill name is required.");
    }

    [Fact]
    public void Validator_ForNullName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = null!
        };

        var validator = new CreateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_ForNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = new string('x', 256) // Exceeds 255 character limit
        };

        var validator = new CreateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Skill name must not exceed 255 characters.");
    }

    [Fact]
    public void Validator_ForWhitespaceName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = "   " // Only whitespace
        };

        var validator = new CreateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Skill name cannot be empty or whitespace.");
    }

    [Fact]
    public void Validator_ForValidNameAt255Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = new string('x', 255) // Exactly 255 characters
        };

        var validator = new CreateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }
}