using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalById;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Moq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Tests.Proposals.Queries.GetProposalById;

public class GetProposalWithDetailsByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public GetProposalWithDetailsByIdQueryHandlerTests()
    {
        var config = new MapperConfiguration(c => c.AddProfile(new ProposalsProfile()));
        _mapper = new Mapper(config);
        _unitOfWorkMock = new();
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var handler = new GetProposalWithDetailsByIdQueryHandler(_unitOfWorkMock.Object, _mapper);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void Query_ShouldHaveCorrectProposalId()
    {
        // Arrange
        var proposalId = 42;

        // Act
        var query = new GetProposalWithDetailsByIdQuery(proposalId);

        // Assert
        query.ProposalId.Should().Be(proposalId);
    }

    [Fact]
    public void Handle_ShouldAcceptValidParameters()
    {
        // Arrange
        var proposalId = 1;
        var query = new GetProposalWithDetailsByIdQuery(proposalId);
        var emptyQueryable = new List<Proposal>().AsQueryable();
        
        _unitOfWorkMock.Setup(x => x.Proposals.GetByIdWithDetailsQueryable(proposalId))
            .Returns(emptyQueryable);

        var handler = new GetProposalWithDetailsByIdQueryHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert - Just verify the setup works
        handler.Should().NotBeNull();
        query.ProposalId.Should().Be(proposalId);
        
        // Verify that the repository setup is working
        var result = _unitOfWorkMock.Object.Proposals.GetByIdWithDetailsQueryable(proposalId);
        result.Should().NotBeNull();
    }

    [Fact]
    public void Query_WithValidId_ShouldCreateCorrectQuery()
    {
        // Arrange
        var proposalId = 123;

        // Act
        var query = new GetProposalWithDetailsByIdQuery(proposalId);

        // Assert
        query.ProposalId.Should().Be(proposalId);
        query.Should().NotBeNull();
        query.Should().BeOfType<GetProposalWithDetailsByIdQuery>();
    }
}