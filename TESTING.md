# Testing Guide

This repo uses xUnit, Moq, and FluentAssertions for unit tests. Some tests may also use FakeItEasy. Tests currently focus on the Application layer (CQRS handlers, repositories abstractions).

## Projects
- tests/FreelanceJobBoard.Application.Tests — main test project

## Commands
```bash
# Run all tests
 dotnet test
# Run only application tests
 dotnet test ./tests/FreelanceJobBoard.Application.Tests
# Collect code coverage
 dotnet test --collect:"XPlat Code Coverage"
```

## Example patterns

xUnit + FluentAssertions
```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldSucceed()
{
    // Arrange
    var handler = CreateHandler();
    var cmd = new CreateJobCommand { Title = "Test", BudgetMin = 100, BudgetMax = 200 };

    // Act
    var id = await handler.Handle(cmd, CancellationToken.None);

    // Assert
    id.Should().BeGreaterThan(0);
}
```

Moq usage
```csharp
var uow = new Mock<IUnitOfWork>();
uow.Setup(x => x.Jobs.CreateAsync(It.IsAny<Job>()))
   .Returns(Task.CompletedTask);
```

FluentAssertions for collections
```csharp
var result = await handler.Handle(query, CancellationToken.None);
result.Should().NotBeNull().And.HaveCountGreaterThan(0);
```

## Integration testing (suggested)
- Add WebApplicationFactory for API to test controllers
- Use Testcontainers or SQL Server LocalDB for EF integration tests
- TODO: Add API integration test project if/when needed

## Coverage
- Use built-in collector or add coverlet.collector package to the test project
```bash
dotnet add tests/FreelanceJobBoard.Application.Tests package coverlet.collector
```

## Test data and fakes
- Prefer builders to create entities
- Mock ICurrentUserService for authenticated scenarios
- Use InMemory provider for simple EF tests, but prefer a real SQL instance for critical queries
