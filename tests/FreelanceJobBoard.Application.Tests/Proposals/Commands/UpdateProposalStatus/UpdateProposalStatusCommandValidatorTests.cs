using FluentValidation.TestHelper;
using FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Application.Tests.Proposals.Commands.UpdateProposalStatus;

public class UpdateProposalStatusCommandValidatorTests
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange 
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 1,
            Status = ProposalStatus.Accepted,
            ClientFeedback = "Great proposal!"
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.ProposalId);
        result.ShouldNotHaveValidationErrorFor(c => c.Status);
        result.ShouldNotHaveValidationErrorFor(c => c.ClientFeedback);
    }

    [Fact]
    public void Validator_ForInvalidProposalId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 0,
            Status = ProposalStatus.Accepted,
            ClientFeedback = "Great proposal!"
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ProposalId)
            .WithErrorMessage("Proposal ID must be greater than 0.");
    }

    [Fact]
    public void Validator_ForNegativeProposalId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = -1,
            Status = ProposalStatus.Accepted
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ProposalId);
    }

    [Fact]
    public void Validator_ForEmptyStatus_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 1,
            Status = "",
            ClientFeedback = "Test feedback"
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Status)
            .WithErrorMessage("Status is required.");
    }

    [Fact]
    public void Validator_ForInvalidStatus_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 1,
            Status = "InvalidStatus",
            ClientFeedback = "Test feedback"
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Status);
    }

    [Theory]
    [InlineData(ProposalStatus.Accepted)]
    [InlineData(ProposalStatus.Rejected)]
    [InlineData(ProposalStatus.Pending)]
    [InlineData(ProposalStatus.UnderReview)]
    public void Validator_ForValidStatuses_ShouldNotHaveValidationError(string status)
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 1,
            Status = status,
            ClientFeedback = "Test feedback"
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Status);
    }

    [Fact]
    public void Validator_ForClientFeedbackTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 1,
            Status = ProposalStatus.Accepted,
            ClientFeedback = new string('x', 1001) // Exceeds 1000 character limit
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ClientFeedback)
            .WithErrorMessage("Client feedback must not exceed 1000 characters.");
    }

    [Fact]
    public void Validator_ForNullClientFeedback_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 1,
            Status = ProposalStatus.Accepted,
            ClientFeedback = null
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.ClientFeedback);
    }

    [Fact]
    public void Validator_ForClientFeedbackAt1000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand
        {
            ProposalId = 1,
            Status = ProposalStatus.Accepted,
            ClientFeedback = new string('x', 1000) // Exactly 1000 characters
        };

        var validator = new UpdateProposalStatusCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.ClientFeedback);
    }
}