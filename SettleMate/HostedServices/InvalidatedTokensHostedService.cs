using SettleMate.Database;
using SettleMate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SettleMate.HostedServices;

/// <summary>
/// The hosted service that creates in-memory cache of invalidated refresh tokens read from DB
/// </summary>
public class InvalidatedTokensHostedService : IHostedService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InvalidatedTokensHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the class
    /// </summary>
    public InvalidatedTokensHostedService(
        IMemoryCache memoryCache,
        IServiceProvider serviceProvider,
        ILogger<InvalidatedTokensHostedService> logger)
    {
        _memoryCache = memoryCache;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading invalidated tokens into memory cache");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var invalidatedTokens = await dbContext.RefreshTokens
            .Where(x => x.Invalidated)
            .ToListAsync(cancellationToken: cancellationToken);

        var tokenCount = 0;
        foreach (var token in invalidatedTokens)
        {
            _memoryCache.Set(token.JwtId, RevocatedTokenType.Invalidated);
            tokenCount++;
        }

        _logger.LogInformation("Loaded {TokenCount} invalidated tokens into memory cache", tokenCount);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InvalidatedTokensHostedService stopping");
        return Task.CompletedTask;
    }
}
