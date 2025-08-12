using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class NotificationSettingsViewModel
{
    // Email Notification Settings
    [Display(Name = "Enable Email Notifications")]
    public bool EmailNotificationsEnabled { get; set; } = true;
    
    [Display(Name = "Job Updates")]
    public bool JobUpdates { get; set; } = true;
    
    [Display(Name = "Proposal Updates")]
    public bool ProposalUpdates { get; set; } = true;
    
    [Display(Name = "Contract Updates")]
    public bool ContractUpdates { get; set; } = true;
    
    [Display(Name = "Payment Updates")]
    public bool PaymentUpdates { get; set; } = true;
    
    [Display(Name = "Review Updates")]
    public bool ReviewUpdates { get; set; } = true;
    
    [Display(Name = "System Updates")]
    public bool SystemUpdates { get; set; } = false;

    // Browser Notification Settings
    [Display(Name = "Enable Browser Notifications")]
    public bool BrowserNotificationsEnabled { get; set; } = true;
    
    [Display(Name = "Browser Job Updates")]
    public bool BrowserJobUpdates { get; set; } = true;
    
    [Display(Name = "Browser Proposal Updates")]
    public bool BrowserProposalUpdates { get; set; } = true;
    
    [Display(Name = "Browser Contract Updates")]
    public bool BrowserContractUpdates { get; set; } = true;
    
    [Display(Name = "Browser Payment Updates")]
    public bool BrowserPaymentUpdates { get; set; } = true;

    // Frequency and Timing Settings
    [Required]
    [Display(Name = "Email Digest Frequency")]
    public string EmailDigestFrequency { get; set; } = "Immediate";
    
    [Display(Name = "Quiet Hours Start")]
    [DataType(DataType.Time)]
    public TimeSpan? QuietHoursStart { get; set; } = new TimeSpan(22, 0, 0); // 10 PM
    
    [Display(Name = "Quiet Hours End")]
    [DataType(DataType.Time)]
    public TimeSpan? QuietHoursEnd { get; set; } = new TimeSpan(8, 0, 0);   // 8 AM

    // User Information
    public string UserId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string FullName { get; set; } = null!;
    
    // Helper method to validate quiet hours
    public bool IsQuietHoursValid()
    {
        if (!QuietHoursStart.HasValue || !QuietHoursEnd.HasValue)
            return true; // Optional fields
            
        // Quiet hours can span midnight, so this is always valid
        return true;
    }
}