using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Skills.Commands.CreateSkill;
using FreelanceJobBoard.Application.Features.Skills.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Skills.Commands.CreateSkill;

public class CreateSkillCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public CreateSkillCommandHandlerTests()
    {
        var config = new MapperConfiguration(c => c.AddProfile(new SkillsProfile()));
        _mapper = new Mapper(config);
        _unitOfWorkMock = new();
    }

    [Fact]
    public async Task Handle_ForValidCommand_ShouldCreateSkillAndReturnId()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = "C# Programming"
        };

        var existingSkills = new List<Skill>();
        var skillId = 1;

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(existingSkills);

        _unitOfWorkMock.Setup(x => x.Skills.CreateAsync(It.IsAny<Skill>()))
            .Callback<Skill>(s => s.Id = skillId)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = new CreateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(skillId);
        
        _unitOfWorkMock.Verify(x => x.Skills.CreateAsync(It.Is<Skill>(s => 
            s.Name == command.Name && 
            s.IsActive == true)), Times.Once);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSkillAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = "C# Programming"
        };

        var existingSkills = new List<Skill>
        {
            new Skill { Id = 1, Name = "C# Programming", IsActive = true }
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(existingSkills);

        var handler = new CreateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A skill with the name 'C# Programming' already exists.");
    }

    [Fact]
    public async Task Handle_WhenSkillExistsWithDifferentCase_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = "c# programming" // Different case
        };

        var existingSkills = new List<Skill>
        {
            new Skill { Id = 1, Name = "C# Programming", IsActive = true }
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(existingSkills);

        var handler = new CreateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A skill with the name 'c# programming' already exists.");
    }

    [Fact]
    public async Task Handle_WhenSkillExistsButInactive_ShouldStillThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = "C# Programming"
        };

        var existingSkills = new List<Skill>
        {
            new Skill { Id = 1, Name = "C# Programming", IsActive = false } // Inactive skill
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(existingSkills);

        var handler = new CreateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A skill with the name 'C# Programming' already exists.");
    }

    [Fact]
    public async Task Handle_WhenSimilarSkillExists_ShouldCreateNewSkill()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = "C# Advanced Programming"
        };

        var existingSkills = new List<Skill>
        {
            new Skill { Id = 1, Name = "C# Programming", IsActive = true } // Similar but different name
        };

        var skillId = 2;

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(existingSkills);

        _unitOfWorkMock.Setup(x => x.Skills.CreateAsync(It.IsAny<Skill>()))
            .Callback<Skill>(s => s.Id = skillId)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = new CreateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(skillId);
        _unitOfWorkMock.Verify(x => x.Skills.CreateAsync(It.IsAny<Skill>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var command = new CreateSkillCommand
        {
            Name = "JavaScript"
        };

        var existingSkills = new List<Skill>();
        Skill? createdSkill = null;

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(existingSkills);

        _unitOfWorkMock.Setup(x => x.Skills.CreateAsync(It.IsAny<Skill>()))
            .Callback<Skill>(s => 
            {
                s.Id = 1;
                createdSkill = s;
            })
            .Returns(Task.CompletedTask);

        var handler = new CreateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        createdSkill.Should().NotBeNull();
        createdSkill!.IsActive.Should().BeTrue();
        createdSkill.Name.Should().Be(command.Name);
    }
}