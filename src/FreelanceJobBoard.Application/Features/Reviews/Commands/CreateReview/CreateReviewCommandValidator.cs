using FluentValidation;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage("Job ID must be greater than 0.");

        RuleFor(x => x.RevieweeId)
            .NotEmpty()
            .WithMessage("Reviewee ID is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment is required.")
            .MaximumLength(1000)
            .WithMessage("Comment must not exceed 1000 characters.");

        RuleFor(x => x.ReviewType)
            .NotEmpty()
            .WithMessage("Review type is required.")
            .Must(BeValidReviewType)
            .WithMessage("Invalid review type. Valid types are: ClientToFreelancer, FreelancerToClient.");
    }

    private static bool BeValidReviewType(string reviewType)
    {
        var validTypes = new[] { ReviewType.ClientToFreelancer, ReviewType.FreelancerToClient };
        return validTypes.Contains(reviewType);
    }
}