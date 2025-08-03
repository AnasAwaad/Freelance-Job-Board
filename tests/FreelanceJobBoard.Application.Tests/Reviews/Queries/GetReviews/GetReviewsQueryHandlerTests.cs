using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviews;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Reviews.Queries.GetReviews;

public class GetReviewsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetReviewsQueryHandler _handler;

    public GetReviewsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetReviewsQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WithJobId_ShouldReturnJobReview()
    {
        // Arrange
        var jobId = 1;
        var query = new GetReviewsQuery { JobId = jobId };

        var review = new Review
        {
            Id = 1,
            JobId = jobId,
            ReviewerId = "client-123",
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer,
            IsVisible = true,
            IsActive = true
        };

        var reviewDto = new ReviewDto
        {
            Id = 1,
            JobId = jobId,
            ReviewerId = "client-123",
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer,
            IsVisible = true
        };

        _unitOfWorkMock.Setup(x => x.Reviews.GetByJobIdAsync(jobId))
            .ReturnsAsync(review);

        _mapperMock.Setup(x => x.Map<IEnumerable<ReviewDto>>(It.IsAny<IEnumerable<Review>>()))
            .Returns(new[] { reviewDto });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Should().BeEquivalentTo(reviewDto);
    }

    [Fact]
    public async Task Handle_WithJobIdNotFound_ShouldReturnEmptyCollection()
    {
        // Arrange
        var jobId = 999;
        var query = new GetReviewsQuery { JobId = jobId };

        _unitOfWorkMock.Setup(x => x.Reviews.GetByJobIdAsync(jobId))
            .ReturnsAsync((Review?)null);

        _mapperMock.Setup(x => x.Map<IEnumerable<ReviewDto>>(It.IsAny<IEnumerable<Review>>()))
            .Returns(Enumerable.Empty<ReviewDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithUserIdAndOnlyVisible_ShouldReturnVisibleUserReviews()
    {
        // Arrange
        var userId = "user-123";
        var query = new GetReviewsQuery { UserId = userId, OnlyVisible = true };

        var reviews = new List<Review>
        {
            new Review
            {
                Id = 1,
                RevieweeId = userId,
                Rating = 5,
                Comment = "Great!",
                IsVisible = true,
                IsActive = true
            },
            new Review
            {
                Id = 2,
                RevieweeId = userId,
                Rating = 4,
                Comment = "Good work!",
                IsVisible = true,
                IsActive = true
            }
        };

        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVisible = r.IsVisible
        }).ToList();

        _unitOfWorkMock.Setup(x => x.Reviews.GetVisibleReviewsByRevieweeIdAsync(userId))
            .ReturnsAsync(reviews);

        _mapperMock.Setup(x => x.Map<IEnumerable<ReviewDto>>(reviews))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(reviewDtos);
    }

    [Fact]
    public async Task Handle_WithUserIdAndNotOnlyVisible_ShouldReturnAllUserReviews()
    {
        // Arrange
        var userId = "user-123";
        var query = new GetReviewsQuery { UserId = userId, OnlyVisible = false };

        var reviews = new List<Review>
        {
            new Review
            {
                Id = 1,
                RevieweeId = userId,
                Rating = 5,
                Comment = "Great!",
                IsVisible = true,
                IsActive = true
            },
            new Review
            {
                Id = 2,
                RevieweeId = userId,
                Rating = 2,
                Comment = "Not good",
                IsVisible = false,
                IsActive = true
            }
        };

        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVisible = r.IsVisible
        }).ToList();

        _unitOfWorkMock.Setup(x => x.Reviews.GetByRevieweeIdAsync(userId))
            .ReturnsAsync(reviews);

        _mapperMock.Setup(x => x.Map<IEnumerable<ReviewDto>>(reviews))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(reviewDtos);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllReviews()
    {
        // Arrange
        var query = new GetReviewsQuery { OnlyVisible = false };

        var allReviews = new List<Review>
        {
            new Review
            {
                Id = 1,
                ReviewerId = "user1",
                RevieweeId = "user2",
                Rating = 5,
                Comment = "Excellent!",
                IsVisible = true,
                IsActive = true
            },
            new Review
            {
                Id = 2,
                ReviewerId = "user3",
                RevieweeId = "user4",
                Rating = 3,
                Comment = "Average",
                IsVisible = false,
                IsActive = true
            }
        };

        var reviewDtos = allReviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            ReviewerId = r.ReviewerId,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVisible = r.IsVisible
        }).ToList();

        _unitOfWorkMock.Setup(x => x.Reviews.GetAllAsync())
            .ReturnsAsync(allReviews);

        _mapperMock.Setup(x => x.Map<IEnumerable<ReviewDto>>(allReviews))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(reviewDtos);
    }

    [Fact]
    public async Task Handle_WithNoFiltersAndOnlyVisible_ShouldReturnOnlyVisibleReviews()
    {
        // Arrange
        var query = new GetReviewsQuery { OnlyVisible = true };

        var allReviews = new List<Review>
        {
            new Review
            {
                Id = 1,
                ReviewerId = "user1",
                RevieweeId = "user2",
                Rating = 5,
                Comment = "Excellent!",
                IsVisible = true,
                IsActive = true
            },
            new Review
            {
                Id = 2,
                ReviewerId = "user3",
                RevieweeId = "user4",
                Rating = 3,
                Comment = "Average",
                IsVisible = false,
                IsActive = true
            },
            new Review
            {
                Id = 3,
                ReviewerId = "user5",
                RevieweeId = "user6",
                Rating = 4,
                Comment = "Good",
                IsVisible = true,
                IsActive = false
            }
        };

        var visibleReviews = allReviews.Where(r => r.IsVisible && r.IsActive).ToList();
        var reviewDtos = visibleReviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            ReviewerId = r.ReviewerId,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVisible = r.IsVisible
        }).ToList();

        _unitOfWorkMock.Setup(x => x.Reviews.GetAllAsync())
            .ReturnsAsync(allReviews);

        _mapperMock.Setup(x => x.Map<IEnumerable<ReviewDto>>(visibleReviews))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }
}