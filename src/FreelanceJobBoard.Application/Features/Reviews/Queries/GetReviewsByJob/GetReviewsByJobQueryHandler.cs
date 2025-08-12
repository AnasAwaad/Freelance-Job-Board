using MediatR;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using AutoMapper;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewsByJob;

public class GetReviewsByJobQueryHandler : IRequestHandler<GetReviewsByJobQuery, IEnumerable<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReviewsByJobQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReviewDto>> Handle(GetReviewsByJobQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Reviews.GetReviewsByJobIdAsync(request.JobId);
        return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }
}