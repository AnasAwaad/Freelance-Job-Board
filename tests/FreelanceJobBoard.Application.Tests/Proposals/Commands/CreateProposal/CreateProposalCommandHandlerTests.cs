using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
namespace FreelanceJobBoard.Application.Tests.Proposals.Commands.CreateProposal;
public class CreateProposalCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<CreateProposalCommandHandler>> _loggerMock;
    private readonly IMapper _mapper;

    public CreateProposalCommandHandlerTests()
    {
        var config = new MapperConfiguration(c => c.AddProfile(new ProposalsProfile()));
        _mapper = new Mapper(config);
        
        _unitOfWorkMock = new();
        _currentUserServiceMock = new();
        _cloudinaryServiceMock = new();
        _notificationServiceMock = new();
        _loggerMock = new();
    }

    [Fact]
    public async Task Handle_ForValidCommand_ShouldCreateProposal()
    {
        // Arrange
        var userId = "user-123";
        var freelancerId = 1;
        var jobId = 1;
        var clientId = 2;

        var command = new CreateProposalCommand
        {
            JobId = jobId,
            UserId = userId,
            CoverLetter = "Test cover letter",
            BidAmount = 1000m,
            EstimatedTimelineDays = 30
        };

        var freelancer = new Freelancer { Id = freelancerId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId, Status = JobStatus.Open };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);
        
        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);
        
        _unitOfWorkMock.Setup(x => x.Proposals.GetProposalsByJobIdAsync(jobId))
            .ReturnsAsync(new List<Proposal>());

        _unitOfWorkMock.Setup(x => x.Proposals.CreateAsync(It.IsAny<Proposal>()))
            .Returns(Task.CompletedTask);
        
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = new CreateProposalCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _cloudinaryServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.Proposals.CreateAsync(It.Is<Proposal>(p => 
            p.JobId == jobId && 
            p.FreelancerId == freelancerId && 
            p.ClientId == clientId &&
            p.Status == ProposalStatus.Submitted)), Times.Once);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _notificationServiceMock.Verify(x => x.NotifyNewProposalAsync(jobId, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateProposalCommand();
        
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new CreateProposalCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _cloudinaryServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User must be authenticated to submit a proposal");
    }

    [Fact]
    public async Task Handle_WhenFreelancerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "user-123";
        var command = new CreateProposalCommand { UserId = userId };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync((Freelancer?)null);

        var handler = new CreateProposalCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _cloudinaryServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Freelancer*");
    }

    [Fact]
    public async Task Handle_WhenJobNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "user-123";
        var jobId = 1;
        var command = new CreateProposalCommand { JobId = jobId, UserId = userId };
        var freelancer = new Freelancer { Id = 1, UserId = userId };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);
        
        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync((Job?)null);

        var handler = new CreateProposalCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _cloudinaryServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Job*");
    }

    [Fact]
    public async Task Handle_WhenJobNotOpen_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = "user-123";
        var jobId = 1;
        var command = new CreateProposalCommand { JobId = jobId, UserId = userId };
        var freelancer = new Freelancer { Id = 1, UserId = userId };
        var job = new Job { Id = jobId, Status = JobStatus.Closed };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);
        
        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        var handler = new CreateProposalCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _cloudinaryServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This job is no longer accepting proposals");
    }

    [Fact]
    public async Task Handle_WhenFreelancerAlreadySubmittedProposal_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = "user-123";
        var freelancerId = 1;
        var jobId = 1;
        var command = new CreateProposalCommand { JobId = jobId, UserId = userId };
        var freelancer = new Freelancer { Id = freelancerId, UserId = userId };
        var job = new Job { Id = jobId, Status = JobStatus.Open };
        var existingProposals = new List<Proposal>
        {
            new Proposal { FreelancerId = freelancerId, JobId = jobId }
        };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);
        
        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);
        
        _unitOfWorkMock.Setup(x => x.Proposals.GetProposalsByJobIdAsync(jobId))
            .ReturnsAsync(existingProposals);

        var handler = new CreateProposalCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _cloudinaryServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You have already submitted a proposal for this job");
    }

    [Fact]
    public async Task Handle_WithPortfolioFiles_ShouldUploadFilesAndCreateAttachments()
    {
        // Arrange
        var userId = "user-123";
        var freelancerId = 1;
        var jobId = 1;
        var clientId = 2;
        var fileUrl = "https://example.com/file.pdf";

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.pdf");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        var command = new CreateProposalCommand
        {
            JobId = jobId,
            UserId = userId,
            CoverLetter = "Test cover letter",
            BidAmount = 1000m,
            EstimatedTimelineDays = 30,
            PortfolioFiles = new List<IFormFile> { mockFile.Object }
        };

        var freelancer = new Freelancer { Id = freelancerId, UserId = userId };
        var job = new Job { Id = jobId, ClientId = clientId, Status = JobStatus.Open };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);
        
        _unitOfWorkMock.Setup(x => x.Jobs.GetByIdAsync(jobId))
            .ReturnsAsync(job);
        
        _unitOfWorkMock.Setup(x => x.Proposals.GetProposalsByJobIdAsync(jobId))
            .ReturnsAsync(new List<Proposal>());

        _cloudinaryServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), "proposals"))
            .ReturnsAsync(fileUrl);

        var handler = new CreateProposalCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _cloudinaryServiceMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _cloudinaryServiceMock.Verify(x => x.UploadFileAsync(mockFile.Object, "proposals"), Times.Once);
        _unitOfWorkMock.Verify(x => x.Proposals.CreateAsync(It.Is<Proposal>(p => 
            p.Attachments != null && p.Attachments.Count == 1)), Times.Once);
    }
}