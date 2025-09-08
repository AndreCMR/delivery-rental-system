using delivery_rental_system;
using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Infra.Persistence;
using delivery_rental_system.Infra.Storage;
using IntegrationTests;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var contentRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "delivery-rental-system"));
        builder.UseContentRoot(contentRoot);

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {           
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll(typeof(DbContextOptions));
         
            var keepAlive = new SqliteConnection("DataSource=:memory:");
            keepAlive.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(keepAlive));

            services.RemoveAll<IFileStorage>();
            services.AddSingleton<IFileStorage, TestFileStorage>();

            services.PostConfigure<MinioOptions>(_ => { });

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<delivery_rental_system.Infra.Messaging.MotorcycleCreatedConsumer>();
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
