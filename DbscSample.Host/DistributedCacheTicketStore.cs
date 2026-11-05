using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace DbscSample.Host;

/// <summary>
/// Stores the full <see cref="AuthenticationTicket"/> (claims, tokens, properties) server-side in an
/// <see cref="IDistributedCache"/>, so the auth cookie only carries a small, data-protected session key.
/// </summary>
public sealed class DistributedCacheTicketStore(IDistributedCache cache, TimeProvider timeProvider) : ITicketStore
{
    private const string KeyPrefix = "auth-ticket:";

    // Fallback lifetime when the ticket has no explicit expiry.
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours(8);

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket, CancellationToken cancellationToken) =>
        StoreAsync(ticket, httpContext: null!, cancellationToken);

    public async Task<string> StoreAsync(AuthenticationTicket ticket, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var key = Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket, httpContext, cancellationToken);
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket) =>
        RenewAsync(key, ticket, CancellationToken.None);

    public Task RenewAsync(string key, AuthenticationTicket ticket, CancellationToken cancellationToken) =>
        RenewAsync(key, ticket, httpContext: null!, cancellationToken);

    public Task RenewAsync(string key, AuthenticationTicket ticket, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = ticket.Properties.ExpiresUtc ?? timeProvider.GetUtcNow().Add(DefaultLifetime)
        };

        var bytes = TicketSerializer.Default.Serialize(ticket);
        return cache.SetAsync(KeyPrefix + key, bytes, options, cancellationToken);
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key) =>
        RetrieveAsync(key, CancellationToken.None);

    public Task<AuthenticationTicket?> RetrieveAsync(string key, CancellationToken cancellationToken) =>
        RetrieveAsync(key, httpContext: null!, cancellationToken);

    public async Task<AuthenticationTicket?> RetrieveAsync(string key, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var bytes = await cache.GetAsync(KeyPrefix + key, cancellationToken);
        return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public Task RemoveAsync(string key) =>
        RemoveAsync(key, CancellationToken.None);

    public Task RemoveAsync(string key, CancellationToken cancellationToken) =>
        RemoveAsync(key, httpContext: null!, cancellationToken);

    public Task RemoveAsync(string key, HttpContext httpContext, CancellationToken cancellationToken) =>
        cache.RemoveAsync(KeyPrefix + key, cancellationToken);
}

/// <summary>
/// Wires the DI-resolved <see cref="ITicketStore"/> into the named cookie scheme's options.
/// </summary>
internal sealed class ConfigureCookieTicketStore(string scheme, ITicketStore store)
    : IPostConfigureOptions<CookieAuthenticationOptions>
{
    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        if (name == scheme)
        {
            options.SessionStore = store;
        }
    }
}

public static class TicketStoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="DistributedCacheTicketStore"/> as the session store for the given cookie scheme.
    /// Swap <c>AddDistributedMemoryCache</c> for Redis/SQL Server in multi-instance deployments.
    /// </summary>
    public static IServiceCollection AddDistributedCacheTicketStore(this IServiceCollection services, string cookieScheme)
    {
        services.AddDistributedMemoryCache(); // Adds in-memory IDistributedCache if no implementation is registered yet.
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITicketStore, DistributedCacheTicketStore>();
        services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>>(sp =>
            new ConfigureCookieTicketStore(cookieScheme, sp.GetRequiredService<ITicketStore>()));
        return services;
    }
}
