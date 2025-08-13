using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class ProposeContractChangeViewModel
{
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Contract Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Project Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Payment amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
    [Display(Name = "Payment Amount")]
    public decimal PaymentAmount { get; set; }

    [Required(ErrorMessage = "Payment type is required")]
    [Display(Name = "Payment Type")]
    public string PaymentType { get; set; } = string.Empty;

    [Display(Name = "Project Deadline")]
    public DateTime? ProjectDeadline { get; set; }

    [StringLength(2000, ErrorMessage = "Deliverables cannot exceed 2000 characters")]
    [Display(Name = "Deliverables")]
    public string? Deliverables { get; set; }

    [StringLength(5000, ErrorMessage = "Terms and conditions cannot exceed 5000 characters")]
    [Display(Name = "Terms and Conditions")]
    public string? TermsAndConditions { get; set; }

    [StringLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters")]
    [Display(Name = "Additional Notes")]
    public string? AdditionalNotes { get; set; }

    [Required(ErrorMessage = "Change reason is required")]
    [StringLength(500, ErrorMessage = "Change reason cannot exceed 500 characters")]
    [Display(Name = "Reason for Changes")]
    public string ChangeReason { get; set; } = string.Empty;

    [Display(Name = "Contract Documents")]
    public List<IFormFile> AttachmentFiles { get; set; } = new List<IFormFile>();
}