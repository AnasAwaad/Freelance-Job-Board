using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetPublicJobDeatils;
public class GetPublicJobDetailsByIdQuery(int jobId) : IRequest<PublicJobDetailsDto?>
{
	public int JobId { get; set; } = jobId;
}