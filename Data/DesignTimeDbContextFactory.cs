using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ConwaysGameOfLifeApi.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GameOfLifeContext>
    {
        public GameOfLifeContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<GameOfLifeContext>();
            
            // Try to get connection string from environment variable first
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__GameOfLifeDatabase");
            
            // Fall back to configuration if environment variable is not set
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = configuration.GetConnectionString("GameOfLifeDatabase");
            }
            
            optionsBuilder.UseNpgsql(connectionString);

            return new GameOfLifeContext(optionsBuilder.Options);
        }
    }
}
