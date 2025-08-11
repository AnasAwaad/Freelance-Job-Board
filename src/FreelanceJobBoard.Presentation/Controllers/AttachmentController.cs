using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class AttachmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AttachmentController> _logger;

    public AttachmentController(ApplicationDbContext context, ILogger<AttachmentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> View(int id)
    {
        try
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null)
            {
                return NotFound("Attachment not found.");
            }

            // Check if user has access to this attachment
            if (!await HasAccessToAttachment(id))
            {
                return Forbid("You don't have access to this attachment.");
            }

            // For PDFs and images, redirect to the file URL for viewing
            if (attachment.FileType == "application/pdf" || attachment.FileType.StartsWith("image/"))
            {
                return Redirect(attachment.FilePath);
            }

            // For other file types, download them
            return await Download(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing attachment {AttachmentId}", id);
            return BadRequest("Error occurred while viewing the attachment.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null)
            {
                return NotFound("Attachment not found.");
            }

            // Check if user has access to this attachment
            if (!await HasAccessToAttachment(id))
            {
                return Forbid("You don't have access to this attachment.");
            }

            // If it's a URL (like from Cloudinary or external source), redirect to it
            if (attachment.FilePath.StartsWith("http://") || attachment.FilePath.StartsWith("https://"))
            {
                return Redirect(attachment.FilePath);
            }

            // If it's a local file path, serve the file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, attachment.FileType, attachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", id);
            return BadRequest("Error occurred while downloading the attachment.");
        }
    }

    private async Task<bool> HasAccessToAttachment(int attachmentId)
    {
        try
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            // Check if the attachment belongs to a proposal the user has access to
            var proposalAttachment = await _context.ProposalAttachments
                .Include(pa => pa.Proposal)
                    .ThenInclude(p => p.Job)
                        .ThenInclude(j => j.Client)
                            .ThenInclude(c => c.User)
                .Include(pa => pa.Proposal)
                    .ThenInclude(p => p.Freelancer)
                        .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(pa => pa.AttachmentId == attachmentId);

            if (proposalAttachment != null)
            {
                // User can access if they are the client who posted the job or the freelancer who made the proposal
                return proposalAttachment.Proposal.Job.Client.User?.Email == userId ||
                       proposalAttachment.Proposal.Freelancer.User?.Email == userId;
            }

            // Check if the attachment belongs to a job the user has access to
            var jobAttachment = await _context.JobAttachments
                .Include(ja => ja.Job)
                    .ThenInclude(j => j.Client)
                        .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(ja => ja.AttachmentId == attachmentId);

            if (jobAttachment != null)
            {
                // Job attachments are accessible to the client who posted the job
                // and potentially to freelancers who are viewing the job (you might want to allow broader access)
                return jobAttachment.Job.Client.User?.Email == userId;
            }

            // If no relationship found, deny access
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access for attachment {AttachmentId}", attachmentId);
            return false;
        }
    }
}