using FreelanceJobBoard.Application.Features.Reviews.Commands.CreateReview;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviews;
using FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }


    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
    {
        var reviewId = await _mediator.Send(command);
        return Ok(new { success = true, data = new { reviewId }, message = "Review created successfully" });
    }

   
    [HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] string? userId, [FromQuery] int? jobId, [FromQuery] bool onlyVisible = true)
    {
        var query = new GetReviewsQuery
        {
            UserId = userId,
            JobId = jobId,
            OnlyVisible = onlyVisible
        };

        var reviews = await _mediator.Send(query);
        return Ok(new { success = true, data = reviews, message = "Reviews retrieved successfully" });
    }


    [HttpGet("summary/{userId}")]
    public async Task<IActionResult> GetReviewSummary(string userId, [FromQuery] int maxRecentReviews = 5)
    {
        var query = new GetReviewSummaryQuery
        {
            UserId = userId,
            MaxRecentReviews = maxRecentReviews
        };

        var summary = await _mediator.Send(query);
        return Ok(new { success = true, data = summary, message = "Review summary retrieved successfully" });
    }

 
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserReviews(string userId, [FromQuery] bool onlyVisible = true)
    {
        var query = new GetReviewsQuery
        {
            UserId = userId,
            OnlyVisible = onlyVisible
        };

        var reviews = await _mediator.Send(query);
        return Ok(new { success = true, data = reviews, message = "User reviews retrieved successfully" });
    }


    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetJobReview(int jobId)
    {
        var query = new GetReviewsQuery
        {
            JobId = jobId,
            OnlyVisible = true
        };

        var reviews = await _mediator.Send(query);
        return Ok(new { success = true, data = reviews.FirstOrDefault(), message = "Job review retrieved successfully" });
    }
}