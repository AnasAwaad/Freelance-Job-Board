using FluentValidation.TestHelper;
using FreelanceJobBoard.Application.Features.Reviews.Commands.CreateReview;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Application.Tests.Reviews.Commands.CreateReview;

public class CreateReviewCommandValidatorTests
{
    private readonly CreateReviewCommandValidator _validator;

    public CreateReviewCommandValidatorTests()
    {
        _validator = new CreateReviewCommandValidator();
    }

    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = "Excellent work!",
            ReviewType = ReviewType.ClientToFreelancer,
            IsVisible = true
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_ForInvalidJobId_ShouldHaveValidationError(int jobId)
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = jobId,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = "Excellent work!",
            ReviewType = ReviewType.ClientToFreelancer
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.JobId)
            .WithErrorMessage("Job ID must be greater than 0.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validator_ForEmptyRevieweeId_ShouldHaveValidationError(string revieweeId)
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = revieweeId,
            Rating = 5,
            Comment = "Excellent work!",
            ReviewType = ReviewType.ClientToFreelancer
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.RevieweeId)
            .WithErrorMessage("Reviewee ID is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void Validator_ForInvalidRating_ShouldHaveValidationError(int rating)
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = rating,
            Comment = "Test comment",
            ReviewType = ReviewType.ClientToFreelancer
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Rating)
            .WithErrorMessage("Rating must be between 1 and 5 stars.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Validator_ForValidRating_ShouldNotHaveValidationError(int rating)
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = rating,
            Comment = "Test comment",
            ReviewType = ReviewType.ClientToFreelancer
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Rating);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validator_ForEmptyComment_ShouldHaveValidationError(string comment)
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = comment,
            ReviewType = ReviewType.ClientToFreelancer
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Comment)
            .WithErrorMessage("Comment is required.");
    }

    [Fact]
    public void Validator_ForCommentTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = new string('x', 1001), 
            ReviewType = ReviewType.ClientToFreelancer
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Comment)
            .WithErrorMessage("Comment must not exceed 1000 characters.");
    }

    [Fact]
    public void Validator_ForCommentAt1000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = new string('x', 1000), 
            ReviewType = ReviewType.ClientToFreelancer
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Comment);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validator_ForEmptyReviewType_ShouldHaveValidationError(string reviewType)
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = "Test comment",
            ReviewType = reviewType
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ReviewType)
            .WithErrorMessage("Review type is required.");
    }

    [Fact]
    public void Validator_ForInvalidReviewType_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = "Test comment",
            ReviewType = "InvalidType"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ReviewType)
            .WithErrorMessage("Invalid review type. Valid types are: ClientToFreelancer, FreelancerToClient.");
    }

    [Theory]
    [InlineData(ReviewType.ClientToFreelancer)]
    [InlineData(ReviewType.FreelancerToClient)]
    public void Validator_ForValidReviewType_ShouldNotHaveValidationError(string reviewType)
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "user-123",
            Rating = 5,
            Comment = "Test comment",
            ReviewType = reviewType
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.ReviewType);
    }
}