using VotingSystem_Claude.Components;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using Microsoft.AspNetCore.Identity;
using VotingSystem_Claude.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SQLite Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Data Source=VotingSystem.db"));

// Add Identity services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

// Register Authentication & Authorization Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVoterService, VoterService>();
builder.Services.AddScoped<IVoterCodeService, VoterCodeService>();

// Register Election Management Services
builder.Services.AddScoped<IElectionService, ElectionService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IVotingService, VotingService>();
builder.Services.AddScoped<IResultsService, ResultsService>();

// Register Student Management Services
builder.Services.AddScoped<IStudentService, StudentService>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
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
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
    await next();
});

// Add standard middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

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