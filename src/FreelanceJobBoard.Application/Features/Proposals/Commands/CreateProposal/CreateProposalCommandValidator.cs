using FluentValidation;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;

public class CreateProposalCommandValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalCommandValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage("Job ID must be greater than 0.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.CoverLetter)
            .MaximumLength(3000)
            .WithMessage("Cover letter must not exceed 3000 characters.");

        RuleFor(x => x.BidAmount)
            .GreaterThan(0)
            .WithMessage("Bid amount must be greater than 0.");

        RuleFor(x => x.EstimatedTimelineDays)
            .GreaterThan(0)
            .WithMessage("Estimated timeline must be greater than 0 days.");
    }
}