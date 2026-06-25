using CourierMax.API.Filters;
using CourierMax.API.Middleware;
using CourierMax.API.Services;
using CourierMax.Application;
using CourierMax.Infrastructure;
using CourierMax.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Threading.RateLimiting;

// Logger de arranque (antes de que se construya el host)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, cfg) =>
        cfg.ReadFrom.Configuration(context.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .WriteTo.Console(outputTemplate:
               "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    // ── DI de Clean Architecture ───────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Controladores ─────────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ── OpenAPI Nativo de .NET 10 ─────────────────────────────────────────────
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((doc, ctx, ct) =>
        {
            doc.Info.Title = "CourierMax API";
            doc.Info.Version = "v1";
            doc.Info.Description = "Sistema de logística de envíos — Clean Architecture .NET 10";
            return Task.CompletedTask;
        });
    });

    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    // ── Limitador de Tasa (Rate Limiting) — 100 req/min por IP ─────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                }));

        options.OnRejected = async (ctx, ct) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await ctx.HttpContext.Response.WriteAsync("Rate limit exceeded. Try again later.", ct);
        };
    });

    // ── Problem Details ───────────────────────────────────────────────────────
    builder.Services.AddProblemDetails();
    builder.Services.AddScoped(typeof(ValidationFilter<>));
    builder.Services.AddScoped<IErrorCodeMapper, ErrorCodeMapper>();

    // ── HTTPS ─────────────────────────────────────────────────────────────────
    builder.Services.AddHttpsRedirection(o => o.HttpsPort = 443);

    var app = builder.Build();

    // ── HTTP Pipeline ─────────────────────────────────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // OpenAPI
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CourierMax API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseSerilogRequestLogging();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseRateLimiter();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // ── Inicialización de la Base de Datos ─────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CourierMaxDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await DbInitializer.InitializeAsync(context, logger);
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
