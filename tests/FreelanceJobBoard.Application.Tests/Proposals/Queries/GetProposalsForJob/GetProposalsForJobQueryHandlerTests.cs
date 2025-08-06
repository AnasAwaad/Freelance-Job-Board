using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalsForJob;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Proposals.Queries.GetProposalsForJob;

public class GetProposalsForJobQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;

    public GetProposalsForJobQueryHandlerTests()
    {
        var config = new MapperConfiguration(c => c.AddProfile(new ProposalsProfile()));
        _mapper = new Mapper(config);
        _unitOfWorkMock = new();
        _currentUserServiceMock = new();
    }

    [Fact]
    public async Task Handle_ForValidQuery_ShouldReturnProposals()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var jobId = 1;

        var query = new GetProposalsForJobQuery
        {
            JobId = jobId
        };

        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId };
        var proposals = new List<Proposal>
        {
            new Proposal 
            { 
                Id = 1, 
                JobId = jobId, 
                Status = ProposalStatus.Submitted,
                CoverLetter = "Proposal 1",
                BidAmount = 1000,
                Attachments = new List<ProposalAttachment>()
            },
            new Proposal 
            { 
                Id = 2, 
                JobId = jobId, 
                Status = ProposalStatus.Accepted,
                CoverLetter = "Proposal 2",
                BidAmount = 1500,
                Attachments = new List<ProposalAttachment>()
            }
        };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        _unitOfWorkMock.Setup(x => x.Proposals.GetProposalsByJobIdAsync(jobId))
            .ReturnsAsync(proposals);

        var handler = new GetProposalsForJobQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var proposalList = result.ToList();
        proposalList[0].Id.Should().Be(1);
        proposalList[0].Status.Should().Be(ProposalStatus.Submitted);
        proposalList[1].Id.Should().Be(2);
        proposalList[1].Status.Should().Be(ProposalStatus.Accepted);

        _unitOfWorkMock.Verify(x => x.Clients.GetByUserIdAsync(userId), Times.Once);
        _unitOfWorkMock.Verify(x => x.Jobs.GetByIdAsync(jobId), Times.Once);
        _unitOfWorkMock.Verify(x => x.Proposals.GetProposalsByJobIdAsync(jobId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnFilteredProposals()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var jobId = 1;

        var query = new GetProposalsForJobQuery
        {
            JobId = jobId,
            Status = ProposalStatus.Submitted
        };

        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId };
        var proposals = new List<Proposal>
        {
            new Proposal 
            { 
                Id = 1, 
                JobId = jobId, 
                Status = ProposalStatus.Submitted,
                Attachments = new List<ProposalAttachment>()
            },
            new Proposal 
            { 
                Id = 2, 
                JobId = jobId, 
                Status = ProposalStatus.Accepted,
                Attachments = new List<ProposalAttachment>()
            },
            new Proposal 
            { 
                Id = 3, 
                JobId = jobId, 
                Status = ProposalStatus.Submitted,
                Attachments = new List<ProposalAttachment>()
            }
        };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        _unitOfWorkMock.Setup(x => x.Proposals.GetProposalsByJobIdAsync(jobId))
            .ReturnsAsync(proposals);

        var handler = new GetProposalsForJobQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only submitted proposals
        result.All(p => p.Status == ProposalStatus.Submitted).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var query = new GetProposalsForJobQuery { JobId = 1 };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new GetProposalsForJobQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User must be authenticated to view job proposals");
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "non-existent-user";
        var query = new GetProposalsForJobQuery { JobId = 1 };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync((Client?)null);

        var handler = new GetProposalsForJobQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenJobNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var jobId = 999;

        var query = new GetProposalsForJobQuery { JobId = jobId };
        var client = new Client { Id = clientId, UserId = userId };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync((Job?)null);

        var handler = new GetProposalsForJobQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenJobNotOwnedByClient_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var otherClientId = 2;
        var jobId = 1;

        var query = new GetProposalsForJobQuery { JobId = jobId };
        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = otherClientId }; // Job owned by different client

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        var handler = new GetProposalsForJobQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only the job owner can view proposals for this job");
    }

    [Fact]
    public async Task Handle_WithEmptyStatusFilter_ShouldReturnAllProposals()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var jobId = 1;

        var query = new GetProposalsForJobQuery
        {
            JobId = jobId,
            Status = string.Empty // Empty status filter
        };

        var client = new Client { Id = clientId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId };
        var proposals = new List<Proposal>
        {
            new Proposal { Id = 1, JobId = jobId, Status = ProposalStatus.Submitted, Attachments = new List<ProposalAttachment>() },
            new Proposal { Id = 2, JobId = jobId, Status = ProposalStatus.Accepted, Attachments = new List<ProposalAttachment>() }
        };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId))
            .ReturnsAsync(client);

        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        _unitOfWorkMock.Setup(x => x.Proposals.GetProposalsByJobIdAsync(jobId))
            .ReturnsAsync(proposals);

        var handler = new GetProposalsForJobQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2); // All proposals returned
    }
}