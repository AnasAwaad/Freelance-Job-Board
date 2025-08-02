namespace FreelanceJobBoard.Application.Interfaces.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
}