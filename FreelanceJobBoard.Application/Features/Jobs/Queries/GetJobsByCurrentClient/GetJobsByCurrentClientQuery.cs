using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobsByCurrentClient;

public class GetJobsByCurrentClientQuery : IRequest<IEnumerable<JobDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
}