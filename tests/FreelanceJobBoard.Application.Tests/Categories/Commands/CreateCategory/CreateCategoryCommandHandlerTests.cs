using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Categories.Commands.CreateCategory;
public class CreateCategoryCommandHandlerTests
{

	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
	private readonly Mock<ILogger<CreateCategoryCommandHandler>> _loggerMock;
	private readonly IMapper _mapper;

	public CreateCategoryCommandHandlerTests()
	{
		var config = new MapperConfiguration(c => c.AddProfile(new CategoriesProfile()));

		_unitOfWorkMock = new();
		_categoryRepositoryMock = new();
		_loggerMock = new();
		_mapper = new Mapper(config);
	}

	[Fact]
	public async Task Handle_ForValidCommand_ReturnCategoryId()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			Name = "test",
			Description = "test"
		};

		var handler = new CreateCategoryCommandHandler(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);

		_unitOfWorkMock.Setup(u => u.Categories)
			.Returns(_categoryRepositoryMock.Object);

		_categoryRepositoryMock.Setup(c => c.CreateAsync(It.IsAny<Category>()))
			.Callback<Category>(c => c.Id = 1)
			.Returns(Task.CompletedTask);

		_unitOfWorkMock.Setup(u => u.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		// Act

		var result = await handler.Handle(command, CancellationToken.None);

		// Assert

		result.Should().Be(1);
		_categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Once);

	}

}
