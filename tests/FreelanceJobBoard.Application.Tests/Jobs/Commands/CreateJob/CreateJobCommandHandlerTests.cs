using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Jobs.Commands.CreateJob;
public class CreateJobCommandHandlerTests
{
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<IJobRepository> _jobRepositoryMock;
	private readonly Mock<ICurrentUserService> _currentUserServiceMock;
	private readonly IMapper _mapper;

	public CreateJobCommandHandlerTests()
	{
		var config = new MapperConfiguration(c => c.AddProfile(new JobsProfile()));

		_unitOfWorkMock = new();
		_jobRepositoryMock = new();
		_mapper = new Mapper(config);
		_currentUserServiceMock = new();
	}

	[Fact]

	public async Task Handle_ForValidCommand_ReturnJobId()
	{
		// Arrange
		var userId = "123";
		var jobId = 1;
		var command = new CreateJobCommand
		{
			Title = "Test",
			Description = "Test",
			BudgetMin = 10,
			BudgetMax = 100,
			Deadline = DateTime.Now.AddDays(1),
			SkillIds = [1, 2, 3],
			CategoryIds = [1, 2, 3],
		};
		var client = new Client { Id = 1, UserId = userId };
		var job = new Job
		{
			Id = jobId,
			Categories = new List<JobCategory>(),
			Skills = new List<JobSkill>()
		};
		Job? resultJob = null;


		_unitOfWorkMock.Setup(u => u.Clients.GetByUserIdAsync(userId))
			.ReturnsAsync(client);

		_currentUserServiceMock.Setup(x => x.UserId)
			.Returns(userId);

		_unitOfWorkMock.Setup(u => u.Categories.GetCategoriesByIdsAsync(command.CategoryIds))
			.ReturnsAsync(new List<Category> { new Category { Id = 1 }, new Category { Id = 2 }, new Category { Id = 3 } });


		_unitOfWorkMock.Setup(u => u.Skills.GetSkillsByIdsAsync(command.SkillIds))
			.ReturnsAsync(new List<Skill> { new Skill { Id = 1 }, new Skill { Id = 2 }, new Skill { Id = 3 } });

		_unitOfWorkMock.Setup(x => x.Jobs.CreateAsync(It.IsAny<Job>()))
			.Callback<Job>(j =>
			{
				j.Id = jobId;
				resultJob = j;
			})
			.Returns(Task.CompletedTask);

		_unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

		var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object, _mapper, _currentUserServiceMock.Object);


		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert

		result.Should().Be(jobId);
		resultJob.ClientId.Should().Be(client.Id);
		resultJob.Categories.Should().HaveCount(3);
		resultJob.Skills.Should().HaveCount(3);

		_unitOfWorkMock.Verify(u => u.Jobs.CreateAsync(It.IsAny<Job>()), Times.Once);
		_unitOfWorkMock.Verify(u => u.Clients.GetByUserIdAsync(userId), Times.Once);
		_unitOfWorkMock.Verify(u => u.Categories.GetCategoriesByIdsAsync(command.CategoryIds), Times.Once);
		_unitOfWorkMock.Verify(u => u.Skills.GetSkillsByIdsAsync(command.SkillIds), Times.Once);
		_unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);

	}

	[Fact]
	public async Task Handle_WhenClientNotFound_ShouldThrowNotFoundException()
	{
		// arrange
		var userId = "user-id";

		_unitOfWorkMock.Setup(u => u.Clients.GetByUserIdAsync(userId))
			.ReturnsAsync((Client?)null);

		_currentUserServiceMock.Setup(c => c.UserId)
			.Returns(userId);

		var command = new CreateJobCommand();
		var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object,
			_mapper,
			_currentUserServiceMock.Object);
		// Act

		var act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert

		await Assert.ThrowsAsync<NotFoundException>(act);

	}

	[Fact]
	public async Task Handle_WhenSomeCategoriesIdsNotFound_ShouldThrowMissingCategoriesException()
	{
		// Arrange
		var userId = "user-id";
		var clientId = 1;

		var command = new CreateJobCommand
		{
			Title = "test",
			CategoryIds = [1, 2, 3]
		};
		var client = new Client()
		{
			Id = clientId
		};

		_unitOfWorkMock.Setup(u => u.Clients.GetByUserIdAsync(userId))
			.ReturnsAsync(client);

		_currentUserServiceMock.Setup(c => c.UserId)
			.Returns(userId);

		_unitOfWorkMock.Setup(u => u.Categories.GetCategoriesByIdsAsync(It.IsAny<IEnumerable<int>>()))
			.ReturnsAsync(new List<Category> { new Category { Id = 1 }, new Category { Id = 2 } });

		var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object,
			_mapper,
			_currentUserServiceMock.Object);

		// Act

		var act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert
		await Assert.ThrowsAsync<MissingCategoriesException>(act);


	}

	[Fact]
	public async Task Handle_WhenSomeSkillsIdsNotFound_ShouldThrowMissingSkillsException()
	{
		// Arrange
		var userId = "user-id";
		var clientId = 1;

		var command = new CreateJobCommand
		{
			Title = "test",
			SkillIds = [1, 2, 3]
		};
		var client = new Client()
		{
			Id = clientId
		};

		_unitOfWorkMock.Setup(u => u.Clients.GetByUserIdAsync(userId))
			.ReturnsAsync(client);

		_currentUserServiceMock.Setup(c => c.UserId)
			.Returns(userId);

		_unitOfWorkMock.Setup(u => u.Skills.GetSkillsByIdsAsync(It.IsAny<IEnumerable<int>>()))
			.ReturnsAsync(new List<Skill> { new Skill { Id = 1 }, new Skill { Id = 2 } });

		var handler = new CreateJobCommandHandler(_unitOfWorkMock.Object,
			_mapper,
			_currentUserServiceMock.Object);

		// Act

		var act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert
		await Assert.ThrowsAsync<MissingSkillsException>(act);


	}

}
