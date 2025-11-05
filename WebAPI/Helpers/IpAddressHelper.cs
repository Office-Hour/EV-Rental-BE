using Microsoft.AspNetCore.Http;

namespace WebAPI.Helpers;

/// <summary>
/// Helper class for retrieving client and server IP addresses
/// </summary>
public static class IpAddressHelper
{
    /// <summary>
    /// Get client IP address from HttpContext
    /// Handles X-Forwarded-For headers for proxy/load balancer scenarios
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>Client IP address or "127.0.0.1" if unavailable</returns>
    public static string GetClientIpAddress(HttpContext httpContext)
    {
        // Check for X-Forwarded-For header (common behind proxies/load balancers)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            var ips = forwardedFor.Split(',');
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Check for X-Real-IP header (common with nginx)
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to RemoteIpAddress
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(remoteIp))
        {
            // Handle IPv6 localhost
            if (remoteIp == "::1")
            {
                return "127.0.0.1";
            }
            return remoteIp;
        }

        // Default fallback
        return "127.0.0.1";
    }

    /// <summary>
    /// Get server/local IP address from HttpContext
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>Server IP address or "127.0.0.1" if unavailable</returns>
    public static string GetServerIpAddress(HttpContext httpContext)
    {
        var localIp = httpContext.Connection.LocalIpAddress?.ToString();
        if (!string.IsNullOrEmpty(localIp))
        {
            // Handle IPv6 localhost
            if (localIp == "::1")
            {
                return "127.0.0.1";
            }
            return localIp;
        }

        // Default fallback
        return "127.0.0.1";
    }
}
