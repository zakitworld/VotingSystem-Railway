using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VotingSystem_Claude.Data;
using Microsoft.Extensions.DependencyInjection;

namespace VotingSystem_Claude.Middleware
{
    public class AuditLoggingOptions
    {
        public bool EnableAuditLogging { get; set; } = true;
        public string AuditLogTableName { get; set; } = "AuditLogs";
        public bool IncludeUserAgent { get; set; } = true;
        public bool IncludeIpAddress { get; set; } = true;
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string Details { get; set; } = null!;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AuditLoggingOptions _options;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(
            RequestDelegate next,
            IOptions<AuditLoggingOptions> options,
            ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.EnableAuditLogging)
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                using (var scope = context.RequestServices.CreateScope())
                {
                    var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await LogRequestAsync(context, scopedContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in audit logging middleware");
                throw;
            }
            finally
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogRequestAsync(HttpContext context, ApplicationDbContext scopedContext)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = context.User?.Identity?.Name ?? "Anonymous",
                    Action = context.Request.Method,
                    EntityType = context.Request.Path.Value?.Split('/')[1] ?? "Unknown",
                    EntityId = context.Request.Path.Value?.Split('/').Last() ?? "Unknown",
                    Details = await GetRequestDetailsAsync(context),
                    IpAddress = _options.IncludeIpAddress ? context.Connection.RemoteIpAddress?.ToString() : null,
                    UserAgent = _options.IncludeUserAgent ? context.Request.Headers["User-Agent"].ToString() : null,
                    Timestamp = DateTime.UtcNow
                };

                scopedContext.Set<AuditLog>().Add(auditLog);
                await scopedContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit entry");
            }
        }

        private async Task<string> GetRequestDetailsAsync(HttpContext context)
        {
            var details = new
            {
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.ToString(),
                Headers = context.Request.Headers
                    .Where(h => !h.Key.StartsWith("Authorization"))
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                StatusCode = context.Response.StatusCode
            };

            return JsonSerializer.Serialize(details);
        }
    }

    public static class AuditLoggingMiddlewareExtensions
    {
        public static IServiceCollection AddAuditLogging(this IServiceCollection services, Action<AuditLoggingOptions> configure)
        {
            services.Configure(configure);
            return services;
        }

        public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuditLoggingMiddleware>();
        }
    }
} 