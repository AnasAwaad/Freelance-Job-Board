using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Domain.Exceptions;
using FreelanceJobBoard.Application.Features.Reviews.Commands.CreateReview;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateReviewCommandHandler>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateReviewCommandHandler>>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _handler = new CreateReviewCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _emailServiceMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User must be authenticated to create a review.");
    }

    [Fact]
    public async Task Handle_WhenUserCannotReviewJob_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer
        };

        var currentUserId = "client-123";
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _unitOfWorkMock.Setup(x => x.Reviews.CanUserReviewJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(false);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You cannot review this job. The job must be completed and you must be either the client or the accepted freelancer.");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyReviewedJob_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer
        };

        var currentUserId = "client-123";
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _unitOfWorkMock.Setup(x => x.Reviews.CanUserReviewJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.Reviews.HasUserReviewedJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You have already reviewed this job.");
    }

    [Fact]
    public async Task Handle_WhenJobNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer
        };

        var currentUserId = "client-123";
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _unitOfWorkMock.Setup(x => x.Reviews.CanUserReviewJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.Reviews.HasUserReviewedJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Jobs.GetJobWithDetailsAsync(command.JobId))
            .ReturnsAsync((Job?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Job with id: 1 doesn't exist");
    }

    [Fact]
    public async Task Handle_WithClientToFreelancerReview_WhenUserIsNotClient_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer
        };

        var currentUserId = "other-user-123";
        var clientUserId = "client-123";
        var freelancerUserId = "freelancer-123";

        var job = CreateJobWithDetails(clientUserId, freelancerUserId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _unitOfWorkMock.Setup(x => x.Reviews.CanUserReviewJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.Reviews.HasUserReviewedJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Jobs.GetJobWithDetailsAsync(command.JobId))
            .ReturnsAsync(job);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only the client can create a ClientToFreelancer review.");
    }

    [Fact]
    public async Task Handle_WithFreelancerToClientReview_WhenUserIsNotFreelancer_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "client-123",
            Rating = 5,
            Comment = "Great client!",
            ReviewType = ReviewType.FreelancerToClient
        };

        var currentUserId = "other-user-123";
        var clientUserId = "client-123";
        var freelancerUserId = "freelancer-123";

        var job = CreateJobWithDetails(clientUserId, freelancerUserId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _unitOfWorkMock.Setup(x => x.Reviews.CanUserReviewJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.Reviews.HasUserReviewedJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Jobs.GetJobWithDetailsAsync(command.JobId))
            .ReturnsAsync(job);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only the accepted freelancer can create a FreelancerToClient review.");
    }

    [Fact]
    public async Task Handle_WithValidClientToFreelancerReview_ShouldCreateReviewSuccessfully()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "freelancer-123",
            Rating = 5,
            Comment = "Great work!",
            ReviewType = ReviewType.ClientToFreelancer,
            IsVisible = true
        };

        var currentUserId = "client-123";
        var freelancerUserId = "freelancer-123";

        var job = CreateJobWithDetails(currentUserId, freelancerUserId);
        var freelancer = new Freelancer { Id = 1, UserId = freelancerUserId, User = new FreelanceJobBoard.Domain.Identity.ApplicationUser { Id = freelancerUserId, Email = "freelancer@test.com", FullName = "Test Freelancer" } };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _unitOfWorkMock.Setup(x => x.Reviews.CanUserReviewJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.Reviews.HasUserReviewedJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Jobs.GetJobWithDetailsAsync(command.JobId))
            .ReturnsAsync(job);

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(freelancerUserId))
            .ReturnsAsync(freelancer);

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdWithDetailsAsync(freelancerUserId))
            .ReturnsAsync(freelancer);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(freelancerUserId))
            .ReturnsAsync((Client?)null);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdWithDetailsAsync(currentUserId))
            .ReturnsAsync(new Client { Id = 1, UserId = currentUserId, User = new FreelanceJobBoard.Domain.Identity.ApplicationUser { Id = currentUserId, Email = "client@test.com", FullName = "Test Client" } });

        _unitOfWorkMock.Setup(x => x.Reviews.GetAverageRatingByRevieweeIdAsync(freelancerUserId))
            .ReturnsAsync(4.5m);

        _unitOfWorkMock.Setup(x => x.Reviews.GetTotalReviewCountByRevieweeIdAsync(freelancerUserId))
            .ReturnsAsync(10);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.Reviews.CreateAsync(It.IsAny<Review>()))
            .Callback<Review>(review => review.Id = 123); 

        _emailServiceMock.Setup(x => x.SendTemplateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        _notificationServiceMock.Setup(x => x.NotifyReviewReceivedAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(123);
        _unitOfWorkMock.Verify(x => x.Reviews.CreateAsync(It.IsAny<Review>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2)); // Once for review creation, once for rating update
        _unitOfWorkMock.Verify(x => x.Freelancers.Update(It.IsAny<Freelancer>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendTemplateEmailAsync(It.IsAny<string>(), "ReviewNotification", It.IsAny<object>()), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyReviewReceivedAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidFreelancerToClientReview_ShouldCreateReviewSuccessfully()
    {
        // Arrange
        var command = new CreateReviewCommand
        {
            JobId = 1,
            RevieweeId = "client-123",
            Rating = 4,
            Comment = "Good client to work with!",
            ReviewType = ReviewType.FreelancerToClient,
            IsVisible = true
        };

        var clientUserId = "client-123";
        var currentUserId = "freelancer-123";

        var job = CreateJobWithDetails(clientUserId, currentUserId);
        var client = new Client { Id = 1, UserId = clientUserId, User = new FreelanceJobBoard.Domain.Identity.ApplicationUser { Id = clientUserId, Email = "client@test.com", FullName = "Test Client" } };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        _unitOfWorkMock.Setup(x => x.Reviews.CanUserReviewJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.Reviews.HasUserReviewedJobAsync(command.JobId, currentUserId))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Jobs.GetJobWithDetailsAsync(command.JobId))
            .ReturnsAsync(job);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(clientUserId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdWithDetailsAsync(clientUserId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(clientUserId))
            .ReturnsAsync((Freelancer?)null);

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdWithDetailsAsync(currentUserId))
            .ReturnsAsync(new Freelancer { Id = 1, UserId = currentUserId, User = new FreelanceJobBoard.Domain.Identity.ApplicationUser { Id = currentUserId, Email = "freelancer@test.com", FullName = "Test Freelancer" } });

        _unitOfWorkMock.Setup(x => x.Reviews.GetAverageRatingByRevieweeIdAsync(clientUserId))
            .ReturnsAsync(4.2m);

        _unitOfWorkMock.Setup(x => x.Reviews.GetTotalReviewCountByRevieweeIdAsync(clientUserId))
            .ReturnsAsync(5);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.Reviews.CreateAsync(It.IsAny<Review>()))
            .Callback<Review>(review => review.Id = 456);

        _emailServiceMock.Setup(x => x.SendTemplateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        _notificationServiceMock.Setup(x => x.NotifyReviewReceivedAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(456);
        _unitOfWorkMock.Verify(x => x.Reviews.CreateAsync(It.IsAny<Review>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
        _unitOfWorkMock.Verify(x => x.Clients.Update(It.IsAny<Client>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendTemplateEmailAsync(It.IsAny<string>(), "ReviewNotification", It.IsAny<object>()), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyReviewReceivedAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    private static Job CreateJobWithDetails(string clientUserId, string freelancerUserId)
    {
        return new Job
        {
            Id = 1,
            ClientId = 1,
            Status = JobStatus.Completed,
            Client = new Client
            {
                Id = 1,
                UserId = clientUserId,
                User = new FreelanceJobBoard.Domain.Identity.ApplicationUser
                {
                    Id = clientUserId,
                    FullName = "Test Client"
                }
            },
            Proposals = new List<Proposal>
            {
                new Proposal
                {
                    Id = 1,
                    JobId = 1,
                    Status = ProposalStatus.Accepted,
                    Freelancer = new Freelancer
                    {
                        Id = 1,
                        UserId = freelancerUserId,
                        User = new FreelanceJobBoard.Domain.Identity.ApplicationUser
                        {
                            Id = freelancerUserId,
                            FullName = "Test Freelancer"
                        }
                    }
                }
            }
        };
    }
}