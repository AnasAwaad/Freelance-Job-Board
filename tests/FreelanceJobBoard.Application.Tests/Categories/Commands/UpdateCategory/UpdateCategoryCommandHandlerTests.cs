using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Categories.Commands.UpdateCategory;
public class UpdateCategoryCommandHandlerTests
{

	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
	private readonly IMapper _mapper;

	public UpdateCategoryCommandHandlerTests()
	{
		var config = new MapperConfiguration(c => c.AddProfile(new CategoriesProfile()));

		_unitOfWorkMock = new();
		_categoryRepositoryMock = new();
		_mapper = new Mapper(config);
	}

	[Fact]
	public async Task Handle_ForValidCommand_ShouldUpdateCategory()
	{
		// Arranage
		var command = new UpdateCategoryCommand
		{
			Id = 1,
			Name = "Test",
			Description = "Test",
		};

		var category = new Category
		{
			Name = "old name",
			Description = "old description"
		};

		_unitOfWorkMock.Setup(u => u.Categories)
			.Returns(_categoryRepositoryMock.Object);

		_categoryRepositoryMock.Setup(c => c.GetByIdAsync(command.Id))
			.ReturnsAsync(category);

		_unitOfWorkMock.Setup(u => u.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		var handler = new UpdateCategoryCommandHandler(_unitOfWorkMock.Object, _mapper);


		// Act
		await handler.Handle(command, CancellationToken.None);

		// Assert

		category.Name.Should().Be(command.Name);
		category.Description.Should().Be(command.Description);

		_categoryRepositoryMock.Verify(r => r.GetByIdAsync(command.Id), Times.Once());
		_unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once());
	}

	[Fact]
	public async Task Handle_ForInvalidCategoryId_ShouldThrowNotFoundException()
	{
		// Arrange

		var command = new UpdateCategoryCommand
		{
			Id = 99
		};

		_unitOfWorkMock.Setup(u => u.Categories)
			.Returns(_categoryRepositoryMock.Object);

		_categoryRepositoryMock.Setup(c => c.GetByIdAsync(command.Id))
			.ReturnsAsync((Category)null);

		var handler = new UpdateCategoryCommandHandler(_unitOfWorkMock.Object, _mapper);

		// Act

		Func<Task> act = () => handler.Handle(command, CancellationToken.None);


		// Assert

		await Assert.ThrowsAsync<NotFoundException>(act);
	}



}
