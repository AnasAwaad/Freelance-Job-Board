using FreelanceJobBoard.API.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace FreelanceJobBoard.API.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
        {
            var endpoint = context.GetEndpoint();
            var attr = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();
            if (attr == null)
            {
                await _next(context);
                return;
            }

            string clientId = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
            string key = $"rl:{clientId}:{context.Request.Path}";
            var current = await cache.GetStringAsync(key);
            int count = string.IsNullOrEmpty(current) ? 0 : int.Parse(current);

            if (count >= attr.MaxRequests)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. Try again later.");
                return;
            }

            await cache.SetStringAsync(key, (count + 1).ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = attr.Window
            });

            await _next(context);
        }
    }
}
