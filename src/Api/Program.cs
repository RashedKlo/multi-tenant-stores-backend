using System.Threading.RateLimiting;
using Api.Hubs;
using Application;
using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.Middleware;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

    builder.Host.UseSerilog((context, services, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithExceptionDetails()
        .WriteTo.File(
            path: "Logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            fileSizeLimitBytes: 50_000_000,
            rollOnFileSizeLimit: true,
            shared: true,
            outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                "{SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
});

// ── Services ──────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, OrderTrackingUserIdProvider>();

builder.Services.AddScoped<IOrderTrackingNotifier, OrderTrackingNotifier>();
builder.Services.AddScoped<ISupportChatNotifier, SupportChatNotifier>();

// CORS — allow Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
      policy.WithOrigins(
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
    options.ColorScheme = StackExchange.Profiling.ColorScheme.Dark;
}).AddEntityFramework();



builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 30;          // max 30 requests
        opt.Window = TimeSpan.FromMinutes(1); // per 1 minute
        opt.QueueLimit = 0;            // no queuing
    });
    options.AddPolicy("auth-login", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(ctx), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, Window = TimeSpan.FromMinutes(15)
            }));

    options.AddPolicy("auth-email", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(ctx), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3, Window = TimeSpan.FromMinutes(10)
            }));

    options.AddPolicy("auth-code", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(ctx), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, Window = TimeSpan.FromMinutes(10)
            }));

    options.AddPolicy("auth-general", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(ctx), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30, Window = TimeSpan.FromMinutes(10)
            }));
});

static string PartitionKey(HttpContext ctx) =>
    ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";


var app = builder.Build();
app.UseMiniProfiler();
// ── Middleware ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
}
app.UseSerilogRequestLogging(opts =>
{
    // Enrich each HTTP request log line with useful context
    opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
    {
        diagCtx.Set("RequestHost", httpCtx.Request.Host.Value);
        diagCtx.Set("UserAgent", httpCtx.Request.Headers.UserAgent.ToString());
        if (httpCtx.User.Identity?.IsAuthenticated == true)
            diagCtx.Set("UserId", httpCtx.User.FindFirst("sub")?.Value);
    };
});

app.UseCors("AllowLocalhost"); // Apply CORS policy
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GuestSessionMiddleware>();
app.UseMiddleware<LanguageMiddleware>();
app.MapHub<OrderTrackingHub>("/hubs/order-tracking"); // currently missing too — add both
app.MapHub<SupportChatHub>("/hubs/support-chat");
app.MapControllers();


app.MapGet("/api/test-cache", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var key = "test:local";

    await db.StringSetAsync(key, $"Hello at {DateTime.UtcNow:O}", TimeSpan.FromMinutes(5));
    var value = await db.StringGetAsync(key);

    return Results.Ok(new
    {
        status = "Cache is working",
        value = value.ToString(),
        ttlSeconds = (await db.KeyTimeToLiveAsync(key))?.TotalSeconds
    });
})
.AllowAnonymous();
try
{
    Log.Information("Starting up multi-tenant-stores-backend");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}