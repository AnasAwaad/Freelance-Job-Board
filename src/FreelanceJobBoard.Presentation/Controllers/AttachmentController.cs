using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using FreelanceJobBoard.Application.Interfaces.Services;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class AttachmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AttachmentController> _logger;
    private readonly ICloudinaryService _cloudinaryService;

    public AttachmentController(ApplicationDbContext context, ILogger<AttachmentController> logger, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
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

            // If it's a URL (like from Cloudinary or external source), try to download it
            if (attachment.FilePath.StartsWith("http://") || attachment.FilePath.StartsWith("https://"))
            {
                try
                {
                    _logger.LogDebug("Attempting to download file from Cloudinary | AttachmentId={AttachmentId}, FilePath={FilePath}", 
                        id, attachment.FilePath);
                        
                    var fileStream = await _cloudinaryService.DownloadFileAsync(attachment.FilePath);
                    
                    // Set proper headers for download
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{attachment.FileName}\"";
                    
                    _logger.LogInformation("Successfully downloaded file from Cloudinary | AttachmentId={AttachmentId}, FileName={FileName}", 
                        id, attachment.FileName);
                    
                    return File(fileStream, attachment.FileType ?? "application/octet-stream", attachment.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Direct download failed, trying browser-based download | AttachmentId={AttachmentId}, FilePath={FilePath}", 
                        id, attachment.FilePath);
                    
                    // For files that can't be downloaded directly (legacy private files),
                    // create a special download page that forces download in the browser
                    return await CreateBrowserDownload(attachment);
                }
            }

            // If it's a local file path, serve the file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, attachment.FileType ?? "application/octet-stream", attachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", id);
            return BadRequest("Error occurred while downloading the attachment.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> DownloadCloudinary(int id)
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

            // This action specifically handles Cloudinary files that might be private
            // by creating a download page that preserves the filename
            var downloadPageHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Download: {System.Web.HttpUtility.HtmlEncode(attachment.FileName)}</title>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; 
            background: #f8f9fa; 
            margin: 0; 
            padding: 20px; 
            display: flex; 
            justify-content: center; 
            align-items: center; 
            min-height: 100vh; 
        }}
        .container {{ 
            background: white; 
            padding: 40px; 
            border-radius: 8px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1); 
            text-align: center; 
            max-width: 500px; 
        }}
        .file-icon {{ 
            font-size: 48px; 
            margin-bottom: 20px; 
            color: #007bff; 
        }}
        .download-btn {{ 
            background: #007bff; 
            color: white; 
            padding: 12px 24px; 
            border: none; 
            border-radius: 4px; 
            font-size: 16px; 
            cursor: pointer; 
            text-decoration: none; 
            display: inline-block; 
            margin: 10px; 
        }}
        .download-btn:hover {{ background: #0056b3; }}
        .file-info {{ 
            background: #f8f9fa; 
            padding: 15px; 
            border-radius: 4px; 
            margin: 20px 0; 
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='file-icon'>??</div>
        <h2>Download File</h2>
        <div class='file-info'>
            <strong>File:</strong> {System.Web.HttpUtility.HtmlEncode(attachment.FileName)}<br>
            <strong>Size:</strong> {(attachment.FileSize / 1024.0 / 1024.0):F1} MB<br>
            <strong>Type:</strong> {System.Web.HttpUtility.HtmlEncode(attachment.FileType ?? "Unknown")}
        </div>
        <a href='{attachment.FilePath}' class='download-btn' target='_blank' rel='noopener'>
            ?? Download File
        </a>
        <br>
        <small style='color: #666; margin-top: 20px; display: block;'>
            If the file opens in your browser instead of downloading, right-click the download button and select ""Save link as..."" or ""Save target as...""
        </small>
    </div>
</body>
</html>";

            _logger.LogInformation("Serving download page for Cloudinary file | AttachmentId={AttachmentId}, FileName={FileName}", 
                attachment.Id, attachment.FileName);

            return Content(downloadPageHtml, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating download page for attachment {AttachmentId}", id);
            return BadRequest("Error occurred while preparing the download.");
        }
    }

    private async Task<IActionResult> CreateBrowserDownload(Domain.Entities.Attachment attachment)
    {
        try
        {
            // Create a temporary view that will force download in the browser
            var downloadHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Downloading {attachment.FileName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
        .download-container {{ max-width: 500px; margin: 0 auto; }}
        .spinner {{ border: 4px solid #f3f3f3; border-top: 4px solid #3498db; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite; margin: 20px auto; }}
        @keyframes spin {{ 0% {{ transform: rotate(0deg); }} 100% {{ transform: rotate(360deg); }} }}
    </style>
</head>
<body>
    <div class='download-container'>
        <h2>Preparing Download</h2>
        <div class='spinner'></div>
        <p>Your file <strong>{attachment.FileName}</strong> will download shortly...</p>
        <p>If the download doesn't start automatically, <a id='download-link' href='{attachment.FilePath}' download='{attachment.FileName}'>click here</a>.</p>
    </div>
    <script>
        // Force download with proper filename
        setTimeout(function() {{
            var link = document.createElement('a');
            link.href = '{attachment.FilePath}';
            link.download = '{attachment.FileName}';
            link.style.display = 'none';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
            // Show success message after a delay
            setTimeout(function() {{
                document.querySelector('.download-container').innerHTML = 
                    '<h2>Download Started!</h2><p>Your file should be downloading now.</p><p><a href=""javascript:window.close()"">Close this window</a></p>';
            }}, 2000);
        }}, 1000);
    </script>
</body>
</html>";

            _logger.LogInformation("Creating browser-based download for legacy file | AttachmentId={AttachmentId}, FileName={FileName}", 
                attachment.Id, attachment.FileName);

            return Content(downloadHtml, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create browser download | AttachmentId={AttachmentId}", attachment.Id);
            
            // Final fallback - simple redirect
            return Redirect(attachment.FilePath);
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

    [HttpGet]
    [Authorize(Roles = "Admin")] // Only allow admins to run this
    public async Task<IActionResult> MigratePrivateFiles()
    {
        try
        {
            var privateAttachments = await _context.Attachments
                .Where(a => a.FilePath.Contains("res.cloudinary.com") && a.FilePath.Contains("/raw/"))
                .ToListAsync();

            var results = new List<string>();
            
            foreach (var attachment in privateAttachments)
            {
                try
                {
                    // Try to access the file first
                    var response = await new HttpClient().GetAsync(attachment.FilePath);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        results.Add($"? PRIVATE: {attachment.FileName} (ID: {attachment.Id}) - {attachment.FilePath}");
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        results.Add($"? PUBLIC: {attachment.FileName} (ID: {attachment.Id}) - Already accessible");
                    }
                    else
                    {
                        results.Add($"?? UNKNOWN: {attachment.FileName} (ID: {attachment.Id}) - Status: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"? ERROR: {attachment.FileName} (ID: {attachment.Id}) - {ex.Message}");
                }
            }

            var reportHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Cloudinary Files Migration Report</title>
    <style>
        body {{ font-family: monospace; padding: 20px; }}
        .success {{ color: green; }}
        .error {{ color: red; }}
        .warning {{ color: orange; }}
    </style>
</head>
<body>
    <h1>Cloudinary Files Report</h1>
    <p>Total files checked: {privateAttachments.Count}</p>
    <pre>{string.Join("\n", results)}</pre>
    
    <h2>Next Steps:</h2>
    <ol>
        <li>Files marked as PRIVATE need to be made public in Cloudinary</li>
        <li>Files marked as PUBLIC are already working</li>
        <li>Files marked as UNKNOWN may need individual investigation</li>
    </ol>
</body>
</html>";

            return Content(reportHtml, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during private files migration check");
            return BadRequest($"Error checking files: {ex.Message}");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")] // Only allow admins to run this
    public async Task<IActionResult> FixPrivateFile(int id)
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

            // Try to access the current URL
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(attachment.FilePath);
            
            string status;
            string action = "";
            
            if (response.IsSuccessStatusCode)
            {
                status = "? PUBLIC - File is already accessible";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                status = "? PRIVATE - File needs to be made public";
                action = @"
                <h3>How to fix this file:</h3>
                <ol>
                    <li>Go to <a href='https://cloudinary.com/console' target='_blank'>Cloudinary Console</a></li>
                    <li>Navigate to Media Library ? proposals folder</li>
                    <li>Find your file: <code>" + attachment.FileName + @"</code></li>
                    <li>Click on the file</li>
                    <li>In the settings, change 'Delivery type' from 'Private' to 'Upload' (public)</li>
                    <li>Save the changes</li>
                </ol>
                <p><strong>Alternatively:</strong> Upload a new version of this file - new uploads are now configured to be public.</p>";
            }
            else
            {
                status = $"?? UNKNOWN STATUS - HTTP {response.StatusCode}";
            }

            var fixHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Fix File: {System.Web.HttpUtility.HtmlEncode(attachment.FileName)}</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; max-width: 800px; margin: 0 auto; }}
        .status {{ padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .public {{ background: #d4edda; color: #155724; border: 1px solid #c3e6cb; }}
        .private {{ background: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; }}
        .unknown {{ background: #fff3cd; color: #856404; border: 1px solid #ffeaa7; }}
        code {{ background: #f8f9fa; padding: 2px 5px; border-radius: 3px; }}
        ol {{ text-align: left; }}
    </style>
</head>
<body>
    <h1>File Status Report</h1>
    <h2>File: {System.Web.HttpUtility.HtmlEncode(attachment.FileName)}</h2>
    <p><strong>ID:</strong> {attachment.Id}</p>
    <p><strong>URL:</strong> <a href='{attachment.FilePath}' target='_blank'>{attachment.FilePath}</a></p>
    
    <div class='status {(response.IsSuccessStatusCode ? "public" : response.StatusCode == System.Net.HttpStatusCode.Unauthorized ? "private" : "unknown")}'>
        <h3>{status}</h3>
    </div>
    
    {action}
    
    <h3>Test Downloads:</h3>
    <p>
        <a href='/Attachment/Download/{id}' class='btn btn-primary'>Standard Download</a> |
        <a href='/Attachment/DownloadCloudinary/{id}' class='btn btn-secondary'>Alternative Download</a>
    </p>
    
    <p><a href='/Attachment/MigratePrivateFiles'>? Back to File Report</a></p>
</body>
</html>";

            return Content(fixHtml, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file status for attachment {AttachmentId}", id);
            return BadRequest($"Error checking file: {ex.Message}");
        }
    }
}