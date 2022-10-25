using GetEmoji.Models;
using Microsoft.EntityFrameworkCore;

namespace GetEmoji.DB;

public class GetEmojiAppContext : DbContext
{
    private readonly ILogger<GetEmojiAppContext> _logger;

    public GetEmojiAppContext(ILogger<GetEmojiAppContext> logger, DbContextOptions<GetEmojiAppContext> options) : base(options)
    {
        _logger = logger;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public DbSet<AppInstallation> AppInstallations { get; set; }

    public async Task InitializeAsync(IConfiguration configuration)
    {
        _logger.LogInformation("Initializing DB");

        await Database.EnsureCreatedAsync();

        var clientId = configuration.GetValue<string>("CLIENT_ID");
        if (!string.IsNullOrEmpty(clientId) && await GetAsync(clientId) is null)
        {
            var appInstallation = new AppInstallation(
                configuration.GetValue<string>("SERVER_URL"),
                clientId,
                configuration.GetValue<string>("CLIENT_SECRET")
            );
            await AddAsync(appInstallation);
            _logger.LogInformation($"Created record for client-id '{clientId}'");
        }
    }

    public async Task<AppInstallation> GetAsync(string clientId)
        => await AppInstallations.FirstOrDefaultAsync(_ => _.ClientId == clientId);

    public async Task AddAsync(AppInstallation appInstallation)
    {
        await AppInstallations.AddAsync(appInstallation);
        await SaveChangesAsync();
    }
}