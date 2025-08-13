using FreelanceJobBoard.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FreelanceJobBoard.Infrastructure.Services;

internal class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string? UserId 
    { 
        get 
        {
            var context = _httpContextAccessor.HttpContext;
            var user = context?.User;
            
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("User is not authenticated");
                return null;
            }

            // Try multiple claim types for user ID
            var userIdClaimTypes = new[]
            {
                ClaimTypes.NameIdentifier,  // Primary standard claim
                "sub",                      // JWT standard claim
                "userId",                   // Custom claim
                "id",                       // Alternative custom claim
                ClaimTypes.Sid              // Security identifier
            };

            string? userId = null;
            foreach (var claimType in userIdClaimTypes)
            {
                userId = user.FindFirstValue(claimType);
                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogDebug("Retrieved UserId: {UserId} from claim type: {ClaimType}", userId, claimType);
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(userId))
            {
                var availableClaims = user.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                _logger.LogWarning("User is authenticated but no user ID claim found. Available claims: {Claims}", 
                    string.Join(", ", availableClaims));
                
                // As a last resort, try to extract from email if available
                var email = user.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Falling back to using email as user identifier: {Email}", email);
                    return email; // This is not ideal but provides a fallback
                }
            }
            
            return userId;
        }
    }

    public string? UserEmail => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}