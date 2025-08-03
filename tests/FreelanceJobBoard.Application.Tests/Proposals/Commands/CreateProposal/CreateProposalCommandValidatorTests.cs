using FluentValidation.TestHelper;
using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Proposals.Commands.CreateProposal;

public class CreateProposalCommandValidatorTests
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange 
        var command = new CreateProposalCommand
        {
            JobId = 1,
            UserId = "user-123",
            CoverLetter = "This is a valid cover letter",
            BidAmount = 1000.50m,
            EstimatedTimelineDays = 30,
            PortfolioFiles = new List<IFormFile>()
        };

        var validator = new CreateProposalCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.JobId);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
        result.ShouldNotHaveValidationErrorFor(c => c.CoverLetter);
        result.ShouldNotHaveValidationErrorFor(c => c.BidAmount);
        result.ShouldNotHaveValidationErrorFor(c => c.EstimatedTimelineDays);
    }

    [Fact]
    public void Validator_ForInvalidCommand_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateProposalCommand
        {
            JobId = 0,
            UserId = "",
            CoverLetter = new string('x', 3001), // Exceeds max length
            BidAmount = -100,
            EstimatedTimelineDays = 0,
            PortfolioFiles = new List<IFormFile>()
        };

        var validator = new CreateProposalCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.JobId);
        result.ShouldHaveValidationErrorFor(c => c.UserId);
        result.ShouldHaveValidationErrorFor(c => c.CoverLetter);
        result.ShouldHaveValidationErrorFor(c => c.BidAmount);
        result.ShouldHaveValidationErrorFor(c => c.EstimatedTimelineDays);
    }

    [Fact]
    public void Validator_ForEmptyUserId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProposalCommand
        {
            JobId = 1,
            UserId = "",
            CoverLetter = "Valid cover letter",
            BidAmount = 1000,
            EstimatedTimelineDays = 30
        };

        var validator = new CreateProposalCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Validator_ForNegativeBidAmount_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProposalCommand
        {
            JobId = 1,
            UserId = "user-123",
            CoverLetter = "Valid cover letter",
            BidAmount = -50,
            EstimatedTimelineDays = 30
        };

        var validator = new CreateProposalCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.BidAmount);
    }

    [Fact]
    public void Validator_ForZeroEstimatedTimelineDays_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProposalCommand
        {
            JobId = 1,
            UserId = "user-123",
            CoverLetter = "Valid cover letter",
            BidAmount = 1000,
            EstimatedTimelineDays = 0
        };

        var validator = new CreateProposalCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.EstimatedTimelineDays);
    }
}