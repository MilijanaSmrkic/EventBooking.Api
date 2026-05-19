using EventBooking.Api.Middleware;
using EventBooking.Application.Abstractions;
using EventBooking.Application.Models;
using EventBooking.Domain.Entities;
using EventBooking.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

// Local startup logger — intentionally not stored in Log.Logger (static) so
// WebApplicationFactory can create multiple host instances in tests without
// hitting the "already frozen" error from ReloadableLogger.
using var startupLogger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog reads full config from appsettings.json after DI is ready.
    builder.Host.UseSerilog((ctx, config) =>
        config.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "EventBooking API",
            Version = "v1",
            Description = "API za upravljanje eventima i rezervacijama."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Unesite JWT token: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                []
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(EventBooking.Application.Events.Commands.Create.CreateEventCommand).Assembly));

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.Configure<JwtSettings>(
        builder.Configuration.GetSection(JwtSettings.SectionName));

    builder.Services.Configure<ReservationApprovedEmailOptions>(
        builder.Configuration.GetSection(ReservationApprovedEmailOptions.SectionName));

    var app = builder.Build();

    await SeedAdminAsync(app);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "EventBooking API v1"));
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms";
    });
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    startupLogger.Fatal(ex, "Application failed to start.");
    throw;
}

static async Task SeedAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("SeedAdmin");

    var adminExists = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
        .AnyAsync(context.Users, u => u.UserName == "admin");

    if (!adminExists)
    {
        context.Users.Add(new User
        {
            UserName = "admin",
            Email = "admin@eventbooking.com",
            PasswordHash = tokenService.HashPassword("Admin123!"),
            Role = UserRoles.Admin
        });
        await context.SaveChangesAsync();
        logger.LogInformation("Admin user seeded (admin@eventbooking.com)");
    }
    else
    {
        logger.LogDebug("Admin user already exists — skipping seed");
    }
}

public partial class Program { }
