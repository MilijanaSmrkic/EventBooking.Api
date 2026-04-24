using EventBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Api.Tests.Factories;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Pre-evaluate the name so every scope in this factory shares the same InMemory store.
            // DbContextOptions<T> has Scoped lifetime — evaluating inside the lambda would
            // produce a new DB name per request, causing SeedAdminAsync and subsequent
            // requests to see different (empty) databases.
            var dbName = "ApiTest_" + Guid.NewGuid();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }
}
