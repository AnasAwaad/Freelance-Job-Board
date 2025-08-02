using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Jobs.Commands.UpdateJob;
public class UpdateJobCommandHandlerTests
{
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<IJobRepository> _jobRepositoryMock;
	private readonly Mock<ICurrentUserService> _currentUserServiceMock;
	private readonly IMapper _mapper;

	public UpdateJobCommandHandlerTests()
	{
		var config = new MapperConfiguration(c => c.AddProfile(new JobsProfile()));

		_unitOfWorkMock = new();
		_jobRepositoryMock = new();
		_mapper = new Mapper(config);
		_currentUserServiceMock = new();
	}

	[Fact]
	public async Task Handle_ForValidCommand_ShouldSaveJob()
	{
		// Arrange
		var userId = "123";
		var command = new UpdateJobCommand
		{
			Id = 1,
			Title = "new title",
			Description = "new desc",
			BudgetMin = 10,
			BudgetMax = 100,
			Deadline = DateTime.Now.AddDays(1),
			SkillIds = [1, 2, 3],
			CategoryIds = [1, 2, 3],
		};
		var client = new Client { Id = 1, UserId = userId };

		var job = new Job
		{
			Title = "old title",
			ClientId = client.Id,
			Categories = new List<JobCategory>(),
			Skills = new List<JobSkill>()
		};
		_unitOfWorkMock.Setup(u => u.Jobs.GetJobWithCategoriesAndSkillsAsync(command.Id))
			.ReturnsAsync(job);

		_unitOfWorkMock.Setup(u => u.Clients.GetByUserIdAsync(userId))
			.ReturnsAsync(client);

		_currentUserServiceMock.Setup(x => x.UserId)
			.Returns(userId);

		_unitOfWorkMock.Setup(u => u.Categories.GetCategoriesByIdsAsync(command.CategoryIds))
			.ReturnsAsync(new List<Category> { new Category { Id = 1 }, new Category { Id = 2 }, new Category { Id = 3 } });


		_unitOfWorkMock.Setup(u => u.Skills.GetSkillsByIdsAsync(command.SkillIds))
			.ReturnsAsync(new List<Skill> { new Skill { Id = 1 }, new Skill { Id = 2 }, new Skill { Id = 3 } });

		_unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

		var handler = new UpdateJobCommandHandler(_unitOfWorkMock.Object, _mapper, _currentUserServiceMock.Object);


		// Act
		await handler.Handle(command, CancellationToken.None);

		// Assert

		job.ClientId.Should().Be(client.Id);
		job.Title.Should().Be(command.Title);
		job.Description.Should().Be(command.Description);
		job.Categories.Should().HaveCount(3);
		job.Skills.Should().HaveCount(3);

		_unitOfWorkMock.Verify(u => u.Clients.GetByUserIdAsync(userId), Times.Once);
		_unitOfWorkMock.Verify(u => u.Categories.GetCategoriesByIdsAsync(command.CategoryIds), Times.Once);
		_unitOfWorkMock.Verify(u => u.Skills.GetSkillsByIdsAsync(command.SkillIds), Times.Once);
		_unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task Handle_WhenJobNotFound_ShouldThrowNotFoundException()
	{
		// arrange
		var command = new UpdateJobCommand
		{
			Id = 1,
		};


		_unitOfWorkMock.Setup(u => u.Jobs.GetJobWithCategoriesAndSkillsAsync(command.Id))
			.ReturnsAsync((Job?)null);


		var handler = new UpdateJobCommandHandler(_unitOfWorkMock.Object,
			_mapper,
			_currentUserServiceMock.Object);
		// Act

		var act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert

		await Assert.ThrowsAsync<NotFoundException>(act);
	}

	[Fact]
	public async Task Handle_WhenClientNotFound_ShouldThrowNotFoundException()
	{
		// arrange

		var command = new UpdateJobCommand();
		var job = new Job();
		var userId = "123";


		_unitOfWorkMock.Setup(u => u.Jobs.GetJobWithCategoriesAndSkillsAsync(command.Id))
			.ReturnsAsync(job);

		_unitOfWorkMock.Setup(u => u.Clients.GetByUserIdAsync(userId))
			.ReturnsAsync((Client?)null);

		var handler = new UpdateJobCommandHandler(_unitOfWorkMock.Object,
			_mapper,
			_currentUserServiceMock.Object);
		// Act

		var act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert

		await Assert.ThrowsAsync<NotFoundException>(act);
	}
}
