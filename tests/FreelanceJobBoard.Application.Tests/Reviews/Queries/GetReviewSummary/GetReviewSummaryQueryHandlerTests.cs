using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Domain.Exceptions;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewSummary;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Reviews.Queries.GetReviewSummary;

public class GetReviewSummaryQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly GetReviewSummaryQueryHandler _handler;

    public GetReviewSummaryQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _userManagerMock = CreateMockUserManager();
        _handler = new GetReviewSummaryQueryHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _userManagerMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnReviewSummary()
    {
        // Arrange
        var userId = "user-123";
        var query = new GetReviewSummaryQuery
        {
            UserId = userId,
            MaxRecentReviews = 3
        };

        var user = new ApplicationUser
        {
            Id = userId,
            FullName = "John Doe"
        };

        var recentReviews = new List<Review>
        {
            new Review
            {
                Id = 1,
                RevieweeId = userId,
                Rating = 5,
                Comment = "Excellent work!",
                IsVisible = true,
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-1)
            },
            new Review
            {
                Id = 2,
                RevieweeId = userId,
                Rating = 4,
                Comment = "Good job!",
                IsVisible = true,
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-2)
            },
            new Review
            {
                Id = 3,
                RevieweeId = userId,
                Rating = 5,
                Comment = "Outstanding!",
                IsVisible = true,
                IsActive = true,
                CreatedOn = DateTime.UtcNow.AddDays(-3)
            }
        };

        var reviewDtos = recentReviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVisible = r.IsVisible,
            CreatedAt = r.CreatedOn
        }).ToList();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Reviews.GetAverageRatingByRevieweeIdAsync(userId))
            .ReturnsAsync(4.67m);

        _unitOfWorkMock.Setup(x => x.Reviews.GetTotalReviewCountByRevieweeIdAsync(userId))
            .ReturnsAsync(15);

        _unitOfWorkMock.Setup(x => x.Reviews.GetVisibleReviewsByRevieweeIdAsync(userId))
            .ReturnsAsync(recentReviews);

        _mapperMock.Setup(x => x.Map<List<ReviewDto>>(It.IsAny<List<Review>>()))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.UserName.Should().Be("John Doe");
        result.AverageRating.Should().Be(4.67m);
        result.TotalReviews.Should().Be(15);
        result.RecentReviews.Should().HaveCount(3);
        result.RecentReviews.Should().BeEquivalentTo(reviewDtos);
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "nonexistent-user";
        var query = new GetReviewSummaryQuery
        {
            UserId = userId,
            MaxRecentReviews = 5
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNoReviews_ShouldReturnEmptySummary()
    {
        // Arrange
        var userId = "user-no-reviews";
        var query = new GetReviewSummaryQuery
        {
            UserId = userId,
            MaxRecentReviews = 5
        };

        var user = new ApplicationUser
        {
            Id = userId,
            FullName = "Jane Doe"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Reviews.GetAverageRatingByRevieweeIdAsync(userId))
            .ReturnsAsync(0m);

        _unitOfWorkMock.Setup(x => x.Reviews.GetTotalReviewCountByRevieweeIdAsync(userId))
            .ReturnsAsync(0);

        _unitOfWorkMock.Setup(x => x.Reviews.GetVisibleReviewsByRevieweeIdAsync(userId))
            .ReturnsAsync(Enumerable.Empty<Review>());

        _mapperMock.Setup(x => x.Map<List<ReviewDto>>(It.IsAny<List<Review>>()))
            .Returns(new List<ReviewDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.UserName.Should().Be("Jane Doe");
        result.AverageRating.Should().Be(0m);
        result.TotalReviews.Should().Be(0);
        result.RecentReviews.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMoreReviewsThanMaxRequested_ShouldLimitResults()
    {
        // Arrange
        var userId = "user-many-reviews";
        var query = new GetReviewSummaryQuery
        {
            UserId = userId,
            MaxRecentReviews = 2 // Requesting only 2 reviews
        };

        var user = new ApplicationUser
        {
            Id = userId,
            FullName = "Popular User"
        };

        var allReviews = new List<Review>
        {
            new Review { Id = 1, RevieweeId = userId, Rating = 5, Comment = "Latest", CreatedOn = DateTime.UtcNow.AddDays(-1) },
            new Review { Id = 2, RevieweeId = userId, Rating = 4, Comment = "Second", CreatedOn = DateTime.UtcNow.AddDays(-2) },
            new Review { Id = 3, RevieweeId = userId, Rating = 3, Comment = "Third", CreatedOn = DateTime.UtcNow.AddDays(-3) },
            new Review { Id = 4, RevieweeId = userId, Rating = 5, Comment = "Fourth", CreatedOn = DateTime.UtcNow.AddDays(-4) }
        };

        var limitedReviews = allReviews.Take(2).ToList();
        var reviewDtos = limitedReviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            RevieweeId = r.RevieweeId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedOn
        }).ToList();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Reviews.GetAverageRatingByRevieweeIdAsync(userId))
            .ReturnsAsync(4.25m);

        _unitOfWorkMock.Setup(x => x.Reviews.GetTotalReviewCountByRevieweeIdAsync(userId))
            .ReturnsAsync(20);

        _unitOfWorkMock.Setup(x => x.Reviews.GetVisibleReviewsByRevieweeIdAsync(userId))
            .ReturnsAsync(allReviews);

        _mapperMock.Setup(x => x.Map<List<ReviewDto>>(It.IsAny<List<Review>>()))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RecentReviews.Should().HaveCount(2);
        result.RecentReviews.First().Comment.Should().Be("Latest");
        result.RecentReviews.Last().Comment.Should().Be("Second");
    }
}