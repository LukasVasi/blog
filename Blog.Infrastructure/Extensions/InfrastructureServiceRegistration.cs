using Blog.Application.Interfaces;
using Blog.Infrastructure.Options;
using Blog.Infrastructure.Persistence.Context;
using Blog.Infrastructure.Persistence.Seeding;
using Blog.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Infrastructure.Extensions
{
    public static class InfrastructureServiceRegistration
    {
        /// <summary>
        /// Adds the database context and all infrastructure layer options and services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">The configuration used when adding services.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database connection string is missing.
        /// </exception>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseInMemoryDb"))
            {
                var sqliteConnection = new SqliteConnection("DataSource=:memory:");
                services.AddSingleton(sqliteConnection);

                services.AddDbContext<BlogDbContext>(options =>
                    options.UseSqlite(sqliteConnection)
                );
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if(string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not specified.");
                }

                services.AddDbContext<BlogDbContext>(options =>
                    options.UseSqlServer(connectionString)
                );
            }

            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            services.Configure<AuthOptions>(configuration.GetSection("Auth"));
            services.Configure<CleanupOptions>(configuration.GetSection("Cleanup"));

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IArticleService, ArticleService>();

            services.AddScoped<ICleanupService, CleanupService>();

            services.AddHostedService<CleanupBackgroundService>();

            return services;
        }

        public static async Task<IApplicationBuilder> InitializeInfrastructureServicesAsync(this IApplicationBuilder application, IConfiguration configuration)
        {
            using var scope = application.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<BlogDbContext>();

            if (configuration.GetValue<bool>("UseInMemoryDb"))
            {
                var connection = services.GetRequiredService<SqliteConnection>();
                await connection.OpenAsync();
                await dbContext.Database.EnsureCreatedAsync();
            }

            await BlogDbContextSeeder.SeedDataAsync(dbContext);

            return application;
        }
    }
}
