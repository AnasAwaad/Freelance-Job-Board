using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposals;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Proposals.Queries.GetFreelancerProposals;

public class GetProposalsForFreelancerQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public GetProposalsForFreelancerQueryHandlerTests()
    {
        var config = new MapperConfiguration(c => c.AddProfile(new ProposalsProfile()));
        _mapper = new Mapper(config);
        _unitOfWorkMock = new();
    }

    [Fact]
    public async Task Handle_ForExistingFreelancer_ShouldReturnProposals()
    {
        // Arrange
        var userId = "user-123";
        var freelancerId = 1;
        var query = new GetProposalsForFreelancerQuery(userId);

        var freelancer = new Freelancer { Id = freelancerId, UserId = userId };
        var proposals = new List<Proposal>
        {
            new Proposal 
            { 
                Id = 1, 
                FreelancerId = freelancerId, 
                JobId = 1,
                CoverLetter = "Test proposal 1",
                BidAmount = 1000m,
                EstimatedTimelineDays = 30,
                Status = "Submitted",
                Attachments = new List<ProposalAttachment>()
            },
            new Proposal 
            { 
                Id = 2, 
                FreelancerId = freelancerId, 
                JobId = 2,
                CoverLetter = "Test proposal 2",
                BidAmount = 1500m,
                EstimatedTimelineDays = 45,
                Status = "Accepted",
                Attachments = new List<ProposalAttachment>()
            }
        };

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);

        _unitOfWorkMock.Setup(x => x.Proposals.GetAllByFreelancerIdAsync(freelancerId))
            .ReturnsAsync(proposals);

        var handler = new GetProposalsForFreelancerQueryHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var proposalList = result.ToList();
        proposalList[0].Id.Should().Be(1);
        proposalList[0].JobId.Should().Be(1);
        proposalList[0].CoverLetter.Should().Be("Test proposal 1");
        proposalList[0].BidAmount.Should().Be(1000m);
        proposalList[0].Status.Should().Be("Submitted");

        proposalList[1].Id.Should().Be(2);
        proposalList[1].JobId.Should().Be(2);
        proposalList[1].CoverLetter.Should().Be("Test proposal 2");
        proposalList[1].BidAmount.Should().Be(1500m);
        proposalList[1].Status.Should().Be("Accepted");

        _unitOfWorkMock.Verify(x => x.Freelancers.GetByUserIdAsync(userId), Times.Once);
        _unitOfWorkMock.Verify(x => x.Proposals.GetAllByFreelancerIdAsync(freelancerId), Times.Once);
    }

    [Fact]
    public async Task Handle_ForNonExistingFreelancer_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = "user-999";
        var query = new GetProposalsForFreelancerQuery(userId);

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync((Freelancer?)null);

        var handler = new GetProposalsForFreelancerQueryHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert
        var act = async () => await handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _unitOfWorkMock.Verify(x => x.Freelancers.GetByUserIdAsync(userId), Times.Once);
        _unitOfWorkMock.Verify(x => x.Proposals.GetAllByFreelancerIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ForFreelancerWithNoProposals_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = "user-123";
        var freelancerId = 1;
        var query = new GetProposalsForFreelancerQuery(userId);

        var freelancer = new Freelancer { Id = freelancerId, UserId = userId };
        var proposals = new List<Proposal>(); // Empty list

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);

        _unitOfWorkMock.Setup(x => x.Proposals.GetAllByFreelancerIdAsync(freelancerId))
            .ReturnsAsync(proposals);

        var handler = new GetProposalsForFreelancerQueryHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _unitOfWorkMock.Verify(x => x.Freelancers.GetByUserIdAsync(userId), Times.Once);
        _unitOfWorkMock.Verify(x => x.Proposals.GetAllByFreelancerIdAsync(freelancerId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapProposalsCorrectly()
    {
        // Arrange
        var userId = "user-123";
        var freelancerId = 1;
        var query = new GetProposalsForFreelancerQuery(userId);

        var freelancer = new Freelancer { Id = freelancerId, UserId = userId };
        var proposal = new Proposal 
        { 
            Id = 1, 
            FreelancerId = freelancerId, 
            JobId = 100,
            CoverLetter = "Detailed cover letter",
            BidAmount = 2500.75m,
            EstimatedTimelineDays = 60,
            Status = "In Review",
            ReviewedAt = DateTime.UtcNow,
            ClientFeedback = "Good proposal",
            Attachments = new List<ProposalAttachment>
            {
                new ProposalAttachment
                {
                    Attachment = new Attachment
                    {
                        Id = 1,
                        FileName = "portfolio.pdf",
                        FilePath = "/files/portfolio.pdf"
                    }
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId))
            .ReturnsAsync(freelancer);

        _unitOfWorkMock.Setup(x => x.Proposals.GetAllByFreelancerIdAsync(freelancerId))
            .ReturnsAsync(new List<Proposal> { proposal });

        var handler = new GetProposalsForFreelancerQueryHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var proposalDto = result.First();
        proposalDto.Id.Should().Be(proposal.Id);
        proposalDto.JobId.Should().Be(proposal.JobId);
        proposalDto.CoverLetter.Should().Be(proposal.CoverLetter);
        proposalDto.BidAmount.Should().Be(proposal.BidAmount);
        proposalDto.EstimatedTimelineDays.Should().Be(proposal.EstimatedTimelineDays);
        proposalDto.Status.Should().Be(proposal.Status);
        proposalDto.ReviewedAt.Should().Be(proposal.ReviewedAt);
        proposalDto.ClientFeedback.Should().Be(proposal.ClientFeedback);
        proposalDto.Attachments.Should().HaveCount(1);
        proposalDto.Attachments.First().Id.Should().Be(1);
        proposalDto.Attachments.First().FileName.Should().Be("portfolio.pdf");
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var handler = new GetProposalsForFreelancerQueryHandler(_unitOfWorkMock.Object, _mapper);

        // Assert
        handler.Should().NotBeNull();
    }
}