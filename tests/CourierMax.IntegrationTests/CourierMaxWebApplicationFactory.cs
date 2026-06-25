using CourierMax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace CourierMax.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that replaces SQL Server with an in-memory SQLite DB.
/// </summary>
public class CourierMaxWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                { "UseSqlite", "true" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Create and open a SqliteConnection that will remain open during the test run
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddSingleton<System.Data.Common.DbConnection>(connection);
        });

        builder.UseEnvironment("Development");
    }
}
