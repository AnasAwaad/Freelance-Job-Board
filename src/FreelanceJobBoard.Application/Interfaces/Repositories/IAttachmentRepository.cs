using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IAttachmentRepository : IGenericRepository<Attachment>
{
    Task<Attachment?> GetAttachmentWithDetailsAsync(int attachmentId);
    Task<IEnumerable<Attachment>> GetAttachmentsByProposalIdAsync(int proposalId);
    Task<IEnumerable<Attachment>> GetAttachmentsByJobIdAsync(int jobId);
}