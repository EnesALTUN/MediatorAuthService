using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace MediatorAuthService.Api.Extensions;

internal static class ForwardedHeadersExtension
{
    /// <summary>
    /// Uses X-Forwarded-For / X-Forwarded-Proto from the reverse proxies listed under ForwardedHeaders:KnownProxies
    /// (IP addresses) and ForwardedHeaders:KnownNetworks (CIDR ranges), so that the client IP used for rate limiting
    /// and logging is the real one. Headers are ignored when they come from any other address; with no proxy
    /// configured the middleware is not added at all, so clients cannot spoof their address.
    /// </summary>
    public static WebApplication UseConfiguredForwardedHeaders(this WebApplication app)
    {
        string[] knownProxies = app.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
        string[] knownNetworks = app.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [];

        if (knownProxies.Length == 0 && knownNetworks.Length == 0)
            return app;

        ForwardedHeadersOptions options = new()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        foreach (string proxy in knownProxies)
            options.KnownProxies.Add(IPAddress.Parse(proxy));

        foreach (string network in knownNetworks)
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));

        app.UseForwardedHeaders(options);

        return app;
    }
}
