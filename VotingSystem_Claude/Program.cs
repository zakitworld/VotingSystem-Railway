using VotingSystem_Claude.Components;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text;
using VotingSystem_Claude.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SQL Server Database with retry policy
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    options.UseSqlServer(connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null));
});

// Add Identity services with enhanced security
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// Add IP restrictions
builder.Services.AddIpRestriction(options =>
{
    options.AllowedIpAddresses = builder.Configuration.GetSection("Security:AllowedIpAddresses").Get<string[]>() ?? Array.Empty<string>();
    options.BlockedIpAddresses = builder.Configuration.GetSection("Security:BlockedIpAddresses").Get<string[]>() ?? Array.Empty<string>();
});

// Add audit logging
builder.Services.AddAuditLogging(options =>
{
    options.EnableAuditLogging = true;
    options.AuditLogTableName = "AuditLogs";
    options.IncludeUserAgent = true;
    options.IncludeIpAddress = true;
});

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Add caching
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// Register Authentication & Authorization Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVoterService, VoterService>();
builder.Services.AddScoped<IVoterCodeService, VoterCodeService>();
builder.Services.AddHttpContextAccessor();

// Register Election Management Services
builder.Services.AddScoped<IElectionService, ElectionService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IVotingService, VotingService>();
builder.Services.AddScoped<IResultsService, ResultsService>();

// Register Student Management Services
builder.Services.AddScoped<IStudentService, StudentService>();

// Register Analytics Service
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Register Theme Service
builder.Services.AddScoped<ThemeService>();

// Register Utility Services
builder.Services.AddScoped<IRetryService, RetryService>();
builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
builder.Services.AddScoped<IRealTimeStatsService, RealTimeStatsService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Add logging with structured logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventSourceLogger();
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Debug);
});

// Configure Forwarded Headers for hosting environments
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Use Forwarded Headers as early as possible
app.UseForwardedHeaders();

// Ensure database is created and migrations are applied
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Use migrations instead of EnsureCreated to ensure all tables are created
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
        
        // Create default admin if none exists, or reset existing admin for development  
        var adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        if (app.Environment.IsDevelopment())
        {
            // In development, always ensure we have a working admin account
            var existingAdmin = await adminService.GetAdminByUsernameAsync("admin");
            // Use configuration for admin password, with a fallback for local development only
            var configPassword = builder.Configuration["SeedData:AdminPassword"];
            var testPassword = !string.IsNullOrEmpty(configPassword) && configPassword != "REPLACE_WITH_SECURE_PASSWORD" 
                ? configPassword 
                : "Admin@123456"; // Default for development if not configured
            
            if (existingAdmin != null)
            {
                // Reset existing admin - unlock and reset password
                var success = await adminService.ResetPasswordAsync("admin", testPassword);
                if (success)
                {
                    logger.LogWarning("DEVELOPMENT: Admin password reset - Username: admin, Password: {Password}", testPassword);
                }
                else
                {
                    logger.LogError("Could not reset admin password");
                }
            }
            else
            {
                // Create new admin
                var admin = new Admin
                {
                    Username = "admin",
                    Email = "admin@school.com",
                    FullName = "System Administrator",
                    IsActive = true
                };
                
                var success = await adminService.CreateAdminAsync(admin, testPassword);
                if (success)
                {
                    logger.LogWarning("DEVELOPMENT: Admin created - Username: admin, Password: {Password}", testPassword);
                }
            }
            logger.LogWarning("CHANGE THESE CREDENTIALS IMMEDIATELY FOR PRODUCTION!");
        }
        else
        {
            // Production: ensure we have an admin and it's unlocked
            var existingAdmin = await adminService.GetAdminByUsernameAsync("admin");
            if (existingAdmin == null)
            {
                var admin = new Admin
                {
                    Username = "admin",
                    Email = "admin@school.com",
                    FullName = "System Administrator",
                    IsActive = true
                };
                
                var prodAdminPassword = builder.Configuration["SeedData:AdminPassword"];
                if (string.IsNullOrEmpty(prodAdminPassword) || prodAdminPassword == "REPLACE_WITH_SECURE_PASSWORD")
                {
                    logger.LogCritical("PRODUCTION: Admin password not configured in SeedData:AdminPassword. Administrator account cannot be created.");
                    // In production, we should probably not have a default password at all if not configured
                    return;
                }
                var success = await adminService.CreateAdminAsync(admin, prodAdminPassword);
                if (success)
                {
                    logger.LogWarning("Default admin created - Username: admin. Please check configuration for password.");
                }
            }
            else
            {
                // Ensure existing admin is unlocked and active
                await adminService.ResetFailedAttemptsAsync("admin");
                if (!existingAdmin.IsActive)
                {
                    existingAdmin.IsActive = true;
                    await adminService.UpdateAdminAsync(existingAdmin);
                }
            }
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "FATAL: Database initialization failed. Check connection string and server permissions.");
    // We continue execution to allow the app to start and serve a friendly error page or health check status
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy",
    "default-src 'self'; " +
    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
    "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
    "img-src 'self' data:; " +
    "font-src 'self' https://cdn.jsdelivr.net; " +
    "connect-src 'self'; " +
    "frame-ancestors 'none'; " +
    "form-action 'self'; " +
    "base-uri 'self'; " +
    "object-src 'none'");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Append("Permissions-Policy", 
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    await next();
});

// Add IP restriction middleware
app.UseIpRestriction();

// Add audit logging middleware
app.UseAuditLogging();

// Add standard middleware
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});
app.UseAntiforgery();

// Add rate limiting middleware
app.UseRateLimiter();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add health checks endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// Add error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred");
        throw;
    }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.Run();