using EventBooking.Application.Abstractions;
using EventBooking.Application.Abstractions.Repositories;
using EventBooking.Application.Models;
using EventBooking.Infrastructure.Persistence;
using EventBooking.Infrastructure.Repositories;
using EventBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventBooking.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IUnitOfWork>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();

            var smtpHost = configuration[$"{SmtpSettings.SectionName}:Host"];
            if (!string.IsNullOrWhiteSpace(smtpHost))
            {
                services.AddOptions<SmtpSettings>()
                    .Bind(configuration.GetSection(SmtpSettings.SectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                services.AddScoped<IEmailQueueService, SmtpEmailService>();
            }
            else
            {
                services.AddScoped<IEmailQueueService, EmailQueueService>();
            }

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<ITokenService, TokenService>();

            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

            return services;
        }
    }
}
