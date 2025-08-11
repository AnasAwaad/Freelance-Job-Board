using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.ProposeContractChange;

public class ProposeContractChangeCommandValidator : AbstractValidator<ProposeContractChangeCommand>
{
    public ProposeContractChangeCommandValidator()
    {
        RuleFor(x => x.ContractId)
            .GreaterThan(0)
            .WithMessage("Contract ID must be greater than 0");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.PaymentAmount)
            .GreaterThan(0)
            .WithMessage("Payment amount must be greater than 0");

        RuleFor(x => x.PaymentType)
            .NotEmpty()
            .WithMessage("Payment type is required")
            .Must(type => new[] { "Fixed", "Hourly", "Milestone" }.Contains(type))
            .WithMessage("Payment type must be Fixed, Hourly, or Milestone");

        RuleFor(x => x.ChangeReason)
            .NotEmpty()
            .WithMessage("Change reason is required")
            .MaximumLength(500)
            .WithMessage("Change reason cannot exceed 500 characters");

        RuleFor(x => x.Deliverables)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Deliverables))
            .WithMessage("Deliverables cannot exceed 2000 characters");

        RuleFor(x => x.TermsAndConditions)
            .MaximumLength(5000)
            .When(x => !string.IsNullOrEmpty(x.TermsAndConditions))
            .WithMessage("Terms and conditions cannot exceed 5000 characters");

        RuleFor(x => x.AdditionalNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.AdditionalNotes))
            .WithMessage("Additional notes cannot exceed 1000 characters");

        // File validation
        RuleForEach(x => x.AttachmentFiles)
            .Must(BeValidFile)
            .WithMessage("Each file must be under 10MB and have a valid format (PDF, DOC, DOCX, JPG, PNG, GIF)")
            .When(x => x.AttachmentFiles?.Any() == true);

        RuleFor(x => x.AttachmentFiles)
            .Must(files => files == null || files.Count <= 10)
            .WithMessage("Maximum 10 files allowed");
    }

    private bool BeValidFile(IFormFile file)
    {
        if (file == null) return true;

        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        var allowedTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/jpeg",
            "image/jpg", 
            "image/png",
            "image/gif"
        };

        return file.Length <= maxFileSize && allowedTypes.Contains(file.ContentType);
    }
}