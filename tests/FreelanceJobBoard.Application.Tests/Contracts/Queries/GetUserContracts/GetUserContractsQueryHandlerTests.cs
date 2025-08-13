using FreelanceJobBoard.Application.Features.Contracts.Queries.GetUserContracts;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FreelanceJobBoard.Application.Tests.Contracts.Queries.GetUserContracts;

public class GetUserContractsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<GetUserContractsQueryHandler>> _loggerMock;
    private readonly GetUserContractsQueryHandler _handler;

    public GetUserContractsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<GetUserContractsQueryHandler>>();
        _handler = new GetUserContractsQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var query = new GetUserContractsQuery { UserId = "" };
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserIsClient_ShouldReturnClientContracts()
    {
        // Arrange
        var userId = "client-123";
        var clientId = 1;
        var query = new GetUserContractsQuery { UserId = userId };

        var client = new Client { Id = clientId, UserId = userId };
        var contracts = new List<Contract>
        {
            new Contract
            {
                Id = 1,
                ClientId = clientId,
                FreelancerId = 2,
                PaymentAmount = 1000,
                ContractStatus = new ContractStatus { Name = "Active" },
                Proposal = new Proposal
                {
                    Job = new Job { Title = "Test Job" }
                },
                Client = new Client
                {
                    User = new Domain.Identity.ApplicationUser { FullName = "Client Name" }
                },
                Freelancer = new Freelancer
                {
                    User = new Domain.Identity.ApplicationUser { FullName = "Freelancer Name" }
                },
                CreatedOn = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId)).ReturnsAsync(client);
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId)).ReturnsAsync((Freelancer?)null);
        _unitOfWorkMock.Setup(x => x.Contracts.GetContractsByClientIdAsync(clientId)).ReturnsAsync(contracts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Contracts);
        Assert.Equal("Test Job", result.Contracts.First().JobTitle);
        Assert.Equal("Freelancer Name", result.Contracts.First().FreelancerName);
    }

    [Fact]
    public async Task Handle_WhenUserIsFreelancer_ShouldReturnFreelancerContracts()
    {
        // Arrange
        var userId = "freelancer-123";
        var freelancerId = 1;
        var query = new GetUserContractsQuery { UserId = userId };

        var freelancer = new Freelancer { Id = freelancerId, UserId = userId };
        var contracts = new List<Contract>
        {
            new Contract
            {
                Id = 1,
                ClientId = 2,
                FreelancerId = freelancerId,
                PaymentAmount = 1000,
                ContractStatus = new ContractStatus { Name = "Active" },
                Proposal = new Proposal
                {
                    Job = new Job { Title = "Test Job" }
                },
                Client = new Client
                {
                    User = new Domain.Identity.ApplicationUser { FullName = "Client Name" }
                },
                Freelancer = new Freelancer
                {
                    User = new Domain.Identity.ApplicationUser { FullName = "Freelancer Name" }
                },
                CreatedOn = DateTime.UtcNow
            }
        };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId)).ReturnsAsync((Client?)null);
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId)).ReturnsAsync(freelancer);
        _unitOfWorkMock.Setup(x => x.Contracts.GetContractsByFreelancerIdAsync(freelancerId)).ReturnsAsync(contracts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Contracts);
        Assert.Equal("Test Job", result.Contracts.First().JobTitle);
        Assert.Equal("Client Name", result.Contracts.First().ClientName);
    }

    [Fact]
    public async Task Handle_WhenUserIsNeitherClientNorFreelancer_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "unknown-123";
        var query = new GetUserContractsQuery { UserId = userId };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _unitOfWorkMock.Setup(x => x.Clients.GetByUserIdAsync(userId)).ReturnsAsync((Client?)null);
        _unitOfWorkMock.Setup(x => x.Freelancers.GetByUserIdAsync(userId)).ReturnsAsync((Freelancer?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }
}