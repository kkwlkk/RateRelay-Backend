using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Authorization;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Infrastructure.Extensions;

public static class AuthExtensions
{
    private const string PermissionPolicyPrefix = "Permission:";

    public static void AddRateRelayAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtAuthOptionsSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtAuthOptionsSection.Get<JwtOptions>();

        if (jwtOptions == null)
        {
            throw new ArgumentNullException(nameof(jwtOptions), "JWT Auth options are not configured.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.Secret)),
                ClockSkew = TimeSpan.Zero
            };
            options.MapInboundClaims = false;
        });

        var authBuilder = services.AddAuthorizationBuilder();
        foreach (var permission in Enum.GetValues<Permission>())
        {
            if (permission == Permission.None)
                continue;

            var policyName = $"{PermissionPolicyPrefix}{permission}";
            authBuilder.AddPolicy(policyName, policy =>
                policy.Requirements.Add(new PermissionRequirement(permission)));
        }

        authBuilder.AddPolicy("RequireVerifiedBusiness", policy =>
            policy.Requirements.Add(new VerifiedBusinessRequirement()));

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserDataResolver, CurrentUserDataResolver>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<CurrentUserContext>();
        services.AddScoped<IAuthorizationHandler, VerifiedBusinessAuthorizationHandler>();
    }
}