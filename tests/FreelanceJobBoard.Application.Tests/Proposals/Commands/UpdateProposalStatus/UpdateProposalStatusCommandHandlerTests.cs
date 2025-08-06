using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Proposals.Commands.UpdateProposalStatus;

public class UpdateProposalStatusCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<UpdateProposalStatusCommandHandler>> _loggerMock;

    public UpdateProposalStatusCommandHandlerTests()
    {
        _unitOfWorkMock = new();
        _currentUserServiceMock = new();
        _notificationServiceMock = new();
        _loggerMock = new();
    }

    [Fact]
    public async Task Handle_ForValidCommand_ShouldUpdateProposalStatus()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var proposalId = 1;
        var jobId = 1;

        var command = new UpdateProposalStatusCommand
        {
            ProposalId = proposalId,
            Status = ProposalStatus.Accepted,
            ClientFeedback = "Great proposal!"
        };

        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId, Status = JobStatus.Open };
        var proposal = new Proposal { Id = proposalId, JobId = jobId, Status = ProposalStatus.Submitted };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Proposals.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        _unitOfWorkMock.Setup(x => x.Proposals.GetAllAsync())
            .ReturnsAsync(new List<Proposal>());

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = new UpdateProposalStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        proposal.Status.Should().Be(ProposalStatus.Accepted);
        proposal.ClientFeedback.Should().Be("Great proposal!");
        proposal.ReviewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        proposal.ReviewedBy.Should().Be(clientId);
        job.Status.Should().Be(JobStatus.InProgress);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyJobStatusChangeAsync(jobId, ProposalStatus.Accepted, "Great proposal!"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateProposalStatusCommand();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new UpdateProposalStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User must be authenticated to manage proposals");
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "non-existent-user";
        var command = new UpdateProposalStatusCommand();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync((Client?)null);

        var handler = new UpdateProposalStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProposalNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var proposalId = 999;

        var command = new UpdateProposalStatusCommand
        {
            ProposalId = proposalId,
            Status = ProposalStatus.Accepted
        };

        var client = new Client { Id = clientId, UserId = userId };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Proposals.GetByIdAsync(proposalId))
            .ReturnsAsync((Proposal?)null);

        var handler = new UpdateProposalStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenJobNotOwnedByClient_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var otherClientId = 2;
        var proposalId = 1;
        var jobId = 1;

        var command = new UpdateProposalStatusCommand
        {
            ProposalId = proposalId,
            Status = ProposalStatus.Accepted
        };

        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = otherClientId }; // Different client owns the job
        var proposal = new Proposal { Id = proposalId, JobId = jobId };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Proposals.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        var handler = new UpdateProposalStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only the job owner can manage proposals for this job");
    }

    [Fact]
    public async Task Handle_WhenStatusAccepted_ShouldRejectOtherProposals()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var proposalId = 1;
        var jobId = 1;

        var command = new UpdateProposalStatusCommand
        {
            ProposalId = proposalId,
            Status = ProposalStatus.Accepted,
            ClientFeedback = "Accepted!"
        };

        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId, Status = JobStatus.Open };
        var acceptedProposal = new Proposal { Id = proposalId, JobId = jobId, Status = ProposalStatus.Submitted };
        var otherProposal1 = new Proposal { Id = 2, JobId = jobId, Status = ProposalStatus.Submitted };
        var otherProposal2 = new Proposal { Id = 3, JobId = jobId, Status = ProposalStatus.Pending };

        var allProposals = new List<Proposal> { acceptedProposal, otherProposal1, otherProposal2 };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Proposals.GetByIdAsync(proposalId))
            .ReturnsAsync(acceptedProposal);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        _unitOfWorkMock.Setup(x => x.Proposals.GetAllAsync())
            .ReturnsAsync(allProposals);

        var handler = new UpdateProposalStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        acceptedProposal.Status.Should().Be(ProposalStatus.Accepted);
        otherProposal1.Status.Should().Be(ProposalStatus.Rejected);
        otherProposal2.Status.Should().Be(ProposalStatus.Rejected);
        
        otherProposal1.ClientFeedback.Should().Be("Job has been assigned to another freelancer");
        otherProposal2.ClientFeedback.Should().Be("Job has been assigned to another freelancer");
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var proposalId = 1;
        var jobId = 1;

        var command = new UpdateProposalStatusCommand
        {
            ProposalId = proposalId,
            Status = "InvalidStatus" // Invalid status
        };

        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId };
        var proposal = new Proposal { Id = proposalId, JobId = jobId };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Proposals.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        var handler = new UpdateProposalStatusCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid status. Valid statuses are: *");
    }
}