using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Auth;

public static class ApiAuthenticationExtensions
{
    /// <summary>
    /// Wires JWT bearer authentication against the Identity service (Duende IdentityServer) and an
    /// authorization policy named after <paramref name="apiScope"/> that requires it as a token scope claim.
    /// </summary>
    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration, string apiScope)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Identity:Authority"];
                options.RequireHttpsMetadata = configuration.GetValue("Identity:RequireHttpsMetadata", true);
                options.TokenValidationParameters.ValidateAudience = true;
                options.Audience = apiScope;
                options.MapInboundClaims = false;
            });

        services.AddAuthorization(options =>
            options.AddPolicy(apiScope, policy => policy.RequireClaim("scope", apiScope)));

        return services;
    }
}
