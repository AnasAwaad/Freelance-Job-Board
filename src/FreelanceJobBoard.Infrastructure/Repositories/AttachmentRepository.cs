using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(ApplicationDbContext context, ILogger<GenericRepository<Attachment>>? logger = null) : base(context, logger)
    {
    }

    public async Task<Attachment?> GetAttachmentWithDetailsAsync(int attachmentId)
    {
        return await _context.Attachments
            .Include(a => a.ProposalAttachments)
                .ThenInclude(pa => pa.Proposal)
            .Include(a => a.JobAttachments)
                .ThenInclude(ja => ja.Job)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.IsActive);
    }

    public async Task<IEnumerable<Attachment>> GetAttachmentsByProposalIdAsync(int proposalId)
    {
        return await _context.ProposalAttachments
            .Include(pa => pa.Attachment)
            .Where(pa => pa.ProposalId == proposalId && pa.Attachment.IsActive)
            .Select(pa => pa.Attachment)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attachment>> GetAttachmentsByJobIdAsync(int jobId)
    {
        return await _context.JobAttachments
            .Include(ja => ja.Attachment)
            .Where(ja => ja.JobId == jobId && ja.Attachment.IsActive)
            .Select(ja => ja.Attachment)
            .ToListAsync();
    }
}