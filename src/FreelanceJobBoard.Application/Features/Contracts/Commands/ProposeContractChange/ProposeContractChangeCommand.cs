using MediatR;
using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.ProposeContractChange;

public class ProposeContractChangeCommand : IRequest<int>
{
    public int ContractId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime? ProjectDeadline { get; set; }
    public string? Deliverables { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? AdditionalNotes { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public List<IFormFile> AttachmentFiles { get; set; } = new List<IFormFile>();
}