using System.Threading.RateLimiting;
using Application;
using Application.Common.Behaviors;
using Infrastructure;
using Infrastructure.Middleware;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// CORS — allow Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
      policy.WithOrigins(
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
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

app.UseCors("AllowLocalhost"); // Apply CORS policy
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GuestSessionMiddleware>();
app.MapControllers();



app.Run();