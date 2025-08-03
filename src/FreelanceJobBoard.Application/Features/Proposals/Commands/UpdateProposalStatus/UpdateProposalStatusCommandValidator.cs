using FluentValidation;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;

public class UpdateProposalStatusCommandValidator : AbstractValidator<UpdateProposalStatusCommand>
{
    public UpdateProposalStatusCommandValidator()
    {
        RuleFor(x => x.ProposalId)
            .GreaterThan(0)
            .WithMessage("Proposal ID must be greater than 0.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(BeValidStatus)
            .WithMessage($"Status must be one of: {string.Join(", ", GetValidStatuses())}");

        RuleFor(x => x.ClientFeedback)
            .MaximumLength(1000)
            .WithMessage("Client feedback must not exceed 1000 characters.");
    }

    private static bool BeValidStatus(string status)
    {
        var validStatuses = GetValidStatuses();
        return validStatuses.Contains(status);
    }

    private static string[] GetValidStatuses()
    {
        return new[] 
        { 
            ProposalStatus.Accepted, 
            ProposalStatus.Rejected, 
            ProposalStatus.Pending, 
            ProposalStatus.UnderReview 
        };
    }
}