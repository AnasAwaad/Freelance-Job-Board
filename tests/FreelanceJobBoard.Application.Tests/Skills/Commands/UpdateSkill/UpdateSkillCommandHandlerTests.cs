using AutoMapper;
using FluentAssertions;
using FreelanceJobBoard.Application.Features.Skills.Commands.UpdateSkill;
using FreelanceJobBoard.Application.Features.Skills.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using Moq;

namespace FreelanceJobBoard.Application.Tests.Skills.Commands.UpdateSkill;

public class UpdateSkillCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public UpdateSkillCommandHandlerTests()
    {
        var config = new MapperConfiguration(c => c.AddProfile(new SkillsProfile()));
        _mapper = new Mapper(config);
        _unitOfWorkMock = new();
    }

    [Fact]
    public async Task Handle_ForValidCommand_ShouldUpdateSkill()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "Updated C# Programming",
            IsActive = false
        };

        var existingSkill = new Skill
        {
            Id = 1,
            Name = "C# Programming",
            IsActive = true,
            CreatedOn = DateTime.UtcNow.AddDays(-10)
        };

        var otherSkills = new List<Skill>
        {
            new Skill { Id = 2, Name = "JavaScript", IsActive = true }
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetByIdAsync(command.Id))
            .ReturnsAsync(existingSkill);

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(otherSkills);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = new UpdateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingSkill.Name.Should().Be(command.Name);
        existingSkill.IsActive.Should().Be(command.IsActive);
        existingSkill.LastUpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _unitOfWorkMock.Verify(x => x.Skills.GetByIdAsync(command.Id), Times.Once);
        _unitOfWorkMock.Verify(x => x.Skills.GetAllAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ForNonExistingSkill_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 999,
            Name = "Non-existing Skill",
            IsActive = true
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetByIdAsync(command.Id))
            .ReturnsAsync((Skill?)null);

        var handler = new UpdateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Message.Contains("Skill") && ex.Message.Contains(command.Id.ToString()));

        _unitOfWorkMock.Verify(x => x.Skills.GetByIdAsync(command.Id), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSkillNameAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "JavaScript", // This name already exists for skill ID 2
            IsActive = true
        };

        var existingSkill = new Skill
        {
            Id = 1,
            Name = "C# Programming",
            IsActive = true
        };

        var allSkills = new List<Skill>
        {
            existingSkill,
            new Skill { Id = 2, Name = "JavaScript", IsActive = true } // Conflicting name
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetByIdAsync(command.Id))
            .ReturnsAsync(existingSkill);

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(allSkills);

        var handler = new UpdateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A skill with the name 'JavaScript' already exists.");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSkillNameAlreadyExistsWithDifferentCase_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "JAVASCRIPT", // Different case but same name
            IsActive = true
        };

        var existingSkill = new Skill
        {
            Id = 1,
            Name = "C# Programming",
            IsActive = true
        };

        var allSkills = new List<Skill>
        {
            existingSkill,
            new Skill { Id = 2, Name = "JavaScript", IsActive = true }
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetByIdAsync(command.Id))
            .ReturnsAsync(existingSkill);

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(allSkills);

        var handler = new UpdateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A skill with the name 'JAVASCRIPT' already exists.");
    }

    [Fact]
    public async Task Handle_WhenUpdatingToSameName_ShouldUpdateSuccessfully()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "C# Programming", // Same name as current
            IsActive = false // Different IsActive value
        };

        var existingSkill = new Skill
        {
            Id = 1,
            Name = "C# Programming",
            IsActive = true
        };

        var allSkills = new List<Skill>
        {
            existingSkill,
            new Skill { Id = 2, Name = "JavaScript", IsActive = true }
        };

        _unitOfWorkMock.Setup(x => x.Skills.GetByIdAsync(command.Id))
            .ReturnsAsync(existingSkill);

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(allSkills);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = new UpdateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingSkill.Name.Should().Be(command.Name);
        existingSkill.IsActive.Should().Be(command.IsActive);
        existingSkill.LastUpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetLastUpdatedOn()
    {
        // Arrange
        var command = new UpdateSkillCommand
        {
            Id = 1,
            Name = "Updated Skill",
            IsActive = true
        };

        var existingSkill = new Skill
        {
            Id = 1,
            Name = "Original Skill",
            IsActive = false,
            CreatedOn = DateTime.UtcNow.AddDays(-5),
            LastUpdatedOn = DateTime.UtcNow.AddDays(-1) // Old update time
        };

        var oldUpdateTime = existingSkill.LastUpdatedOn;

        _unitOfWorkMock.Setup(x => x.Skills.GetByIdAsync(command.Id))
            .ReturnsAsync(existingSkill);

        _unitOfWorkMock.Setup(x => x.Skills.GetAllAsync())
            .ReturnsAsync(new List<Skill> { existingSkill });

        var handler = new UpdateSkillCommandHandler(_unitOfWorkMock.Object, _mapper);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingSkill.LastUpdatedOn.Should().BeAfter(oldUpdateTime!.Value);
        existingSkill.LastUpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}