using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace VotingSystem_Claude.Middleware
{
    public class IpRestrictionOptions
    {
        public string[] AllowedIpAddresses { get; set; } = Array.Empty<string>();
        public string[] BlockedIpAddresses { get; set; } = Array.Empty<string>();
    }

    public class IpRestrictionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IpRestrictionOptions _options;
        private readonly ILogger<IpRestrictionMiddleware> _logger;

        public IpRestrictionMiddleware(
            RequestDelegate next,
            IOptions<IpRestrictionOptions> options,
            ILogger<IpRestrictionMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                _logger.LogWarning("Could not determine IP address for request");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            // Check if IP is blocked
            if (_options.BlockedIpAddresses.Contains(ipAddress))
            {
                _logger.LogWarning("Blocked request from IP: {IpAddress}", ipAddress);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            // If allowed IPs are specified, check if the current IP is allowed
            if (_options.AllowedIpAddresses.Any() && !_options.AllowedIpAddresses.Contains(ipAddress))
            {
                _logger.LogWarning("Rejected request from unauthorized IP: {IpAddress}", ipAddress);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await _next(context);
        }
    }

    public static class IpRestrictionMiddlewareExtensions
    {
        public static IServiceCollection AddIpRestriction(this IServiceCollection services, Action<IpRestrictionOptions> configure)
        {
            services.Configure(configure);
            return services;
        }

        public static IApplicationBuilder UseIpRestriction(this IApplicationBuilder app)
        {
            return app.UseMiddleware<IpRestrictionMiddleware>();
        }
    }
} 