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
        if (string.IsNullOrEmpty(clientId))
        {
            return;
        }

        var appInstallation = new AppInstallation(
            configuration.GetValue<string>("SERVER_URL"),
            clientId,
            configuration.GetValue<string>("CLIENT_SECRET")
        );
        await UpsertAsync(appInstallation);
    }

    public async Task<AppInstallation> GetAsync(string clientId)
        => await AppInstallations.FirstOrDefaultAsync(_ => _.ClientId == clientId);

    public async Task UpsertAsync(AppInstallation appInstallation)
    {
        _logger.LogInformation($"Upserting {nameof(AppInstallation)}: {appInstallation}");
        await AppInstallations.Upsert(appInstallation).RunAsync();
        await SaveChangesAsync();
    }
}