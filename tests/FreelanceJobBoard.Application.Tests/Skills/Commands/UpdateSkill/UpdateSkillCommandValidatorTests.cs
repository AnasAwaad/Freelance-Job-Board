using FluentValidation.TestHelper;
using FreelanceJobBoard.Application.Features.Skills.Commands.UpdateSkill;

namespace FreelanceJobBoard.Application.Tests.Skills.Commands.UpdateSkill;

public class UpdateSkillCommandValidatorTests
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange 
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "C# Programming",
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Id);
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_ForInvalidId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 0,
            Name = "C# Programming",
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Skill ID must be greater than 0.");
    }

    [Fact]
    public void Validator_ForNegativeId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = -1,
            Name = "C# Programming",
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("Skill ID must be greater than 0.");
    }

    [Fact]
    public void Validator_ForEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "",
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

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
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = null!,
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_ForNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = new string('x', 256), // Exceeds 255 character limit
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

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
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "   ", // Only whitespace
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

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
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = new string('x', 255), // Exactly 255 characters
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_ForAllInvalidFields_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 0,
            Name = "",
            IsActive = true
        };

        var validator = new UpdateSkillCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Id);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}