using AutoMapper;
using FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Categories.Commands.DeleteCategory;
public class DeleteCategoryCommandHandlerTests
{
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
	private readonly IMapper _mapper;

	public DeleteCategoryCommandHandlerTests()
	{
		var config = new MapperConfiguration(c => c.AddProfile(new CategoriesProfile()));

		_unitOfWorkMock = new();
		_categoryRepositoryMock = new();
		_mapper = new Mapper(config);
	}

	[Fact]
	public async Task Handle_ForExistingCategory_ShouldDeleteCategory()
	{
		// Arrange
		var command = new DeleteCategoryCommand(100);

		var category = new Category
		{
			Id = command.Id
		};


		_unitOfWorkMock.Setup(u => u.Categories)
			.Returns(_categoryRepositoryMock.Object);

		_categoryRepositoryMock.Setup(c => c.GetByIdAsync(command.Id))
			.ReturnsAsync(category);

		_unitOfWorkMock.Setup(u => u.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		var handler = new DeleteCategoryCommandHandler(_unitOfWorkMock.Object);

		// Act

		await handler.Handle(command, CancellationToken.None);

		// Assert
		_categoryRepositoryMock.Verify(c => c.GetByIdAsync(command.Id), Times.Once);
		_categoryRepositoryMock.Verify(c => c.Delete(It.IsAny<Category>()), Times.Once);
		_unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);

	}


	[Fact]
	public async Task Handle_ForNotExistingCategory_ShouldThrowNotFoundException()
	{
		// Arrange
		var command = new DeleteCategoryCommand(100);

		var category = new Category
		{
			Id = command.Id
		};


		_unitOfWorkMock.Setup(u => u.Categories)
			.Returns(_categoryRepositoryMock.Object);

		_categoryRepositoryMock.Setup(c => c.GetByIdAsync(command.Id))
			.ReturnsAsync((Category?)null);

		var handler = new DeleteCategoryCommandHandler(_unitOfWorkMock.Object);

		// Act

		Func<Task> act = () => handler.Handle(command, CancellationToken.None);


		// Assert
		await Assert.ThrowsAsync<NotFoundException>(act);
		_categoryRepositoryMock.Verify(c => c.GetByIdAsync(command.Id), Times.Once);


	}
}
