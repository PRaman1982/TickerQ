using System;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using TickerQ.Dashboard.Authentication;

namespace TickerQ.Dashboard;

public class DashboardOptionsBuilder
{
    internal string BasePath { get; set; } = "/tickerq/dashboard";
    internal Action<CorsPolicyBuilder> CorsPolicyBuilder { get; set; }
    internal string BackendDomain { get; set; }
    
    // Clean authentication system
    internal AuthConfig Auth { get; set; } = new();
    
    // Custom Middleware Integration
    public Action<IApplicationBuilder> CustomMiddleware { get; set; }
    public Action<IApplicationBuilder> PreDashboardMiddleware { get; set; }
    public Action<IApplicationBuilder> PostDashboardMiddleware { get; set; }
    
    /// <summary>
    /// JsonSerializerOptions specifically for Dashboard API endpoints.
    /// Separate from request serialization options to prevent user configuration from breaking dashboard APIs.
    /// </summary>
    internal JsonSerializerOptions DashboardJsonOptions { get; set; }
    
    // Branding
    internal string BrandName { get; set; } = "TickerQ";
    internal string BrandLogoPath { get; set; } = "./arcenox-logo.svg";
    internal string BrandFaviconPath { get; set; } = "./arcenox-logo.svg";
    
    // When set, API responses with 403 will redirect to this URL
    internal string ForbiddenRedirectUrl { get; set; }
    
    // When set, unauthenticated (401) requests from browsers will be redirected here
    internal string UnauthorizedRedirectUrl { get; set; }
    
    public void SetCorsPolicy(Action<CorsPolicyBuilder> corsPolicyBuilder)
        => CorsPolicyBuilder = corsPolicyBuilder;
    
    public void SetBasePath(string basePath)
        => BasePath = basePath;
    
    public void SetBackendDomain(string backendDomain)
        => BackendDomain = backendDomain;
    
    /// <summary>Configure no authentication (public dashboard)</summary>
    public DashboardOptionsBuilder WithNoAuth()
    {
        Auth.Mode = AuthMode.None;
        return this;
    }

    /// <summary>Enable Basic Authentication with username/password</summary>
    public DashboardOptionsBuilder WithBasicAuth(string username, string password)
    {
        Auth.Mode = AuthMode.Basic;
        Auth.BasicCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return this;
    }
    
    /// <summary>Enable API Key authentication (sent as Bearer token)</summary>
    public DashboardOptionsBuilder WithApiKey(string apiKey)
    {
        Auth.Mode = AuthMode.ApiKey;
        Auth.ApiKey = apiKey;
        return this;
    }
    
    /// <summary>Use the host application's existing authentication system</summary>
    /// <param name="policy">Optional authorization policy name to require (e.g., "AdminPolicy"). If null or empty, uses the default policy.</param>
    public DashboardOptionsBuilder WithHostAuthentication(string? policy = null)
    {
        Auth.Mode = AuthMode.Host;
        Auth.HostAuthorizationPolicy = policy;
        return this;
    }
    
    /// <summary>Configure custom authentication with validation function</summary>
    public DashboardOptionsBuilder WithCustomAuth(Func<string, bool> validator)
    {
        Auth.Mode = AuthMode.Custom;
        Auth.CustomValidator = validator;
        return this;
    }
    
    /// <summary>Set session timeout in minutes</summary>
    public DashboardOptionsBuilder WithSessionTimeout(int minutes)
    {
        Auth.SessionTimeoutMinutes = minutes;
        return this;
    }
    
    /// <summary>
    /// Configure JSON serialization options for Dashboard API endpoints.
    /// These are separate from request serialization to maintain dashboard integrity.
    /// </summary>
    /// <param name="configure">Action to configure JsonSerializerOptions for dashboard APIs</param>
    /// <returns>The DashboardOptionsBuilder for method chaining</returns>
    public DashboardOptionsBuilder ConfigureDashboardJsonOptions(Action<JsonSerializerOptions> configure)
    {
        DashboardJsonOptions ??= new JsonSerializerOptions();
        configure?.Invoke(DashboardJsonOptions);
        return this;
    }

    /// <summary>Set the displayed application name for the dashboard UI</summary>
    public DashboardOptionsBuilder WithBrandName(string name)
    {
        BrandName = name;
        return this;
    }

    /// <summary>Set the logo path (relative to the dashboard wwwroot)</summary>
    public DashboardOptionsBuilder WithBrandLogo(string logoPath)
    {
        BrandLogoPath = logoPath;
        return this;
    }

    /// <summary>Set the favicon path (relative to the dashboard wwwroot)</summary>
    public DashboardOptionsBuilder WithFavicon(string faviconPath)
    {
        BrandFaviconPath = faviconPath;
        return this;
    }

    /// <summary>Set an optional redirect URL for 403 (forbidden) responses</summary>
    public DashboardOptionsBuilder WithForbiddenRedirect(string url)
    {
        ForbiddenRedirectUrl = url;
        return this;
    }

    /// <summary>Set an optional redirect URL for 401 (unauthorized) responses</summary>
    public DashboardOptionsBuilder WithUnauthorizedRedirect(string url)
    {
        UnauthorizedRedirectUrl = url;
        return this;
    }
    
    /// <summary>Validate the authentication configuration</summary>
    internal void Validate()
    {
        Auth.Validate();
    }
}