using System.Text;
using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Infrastructure.Auth;
using GymKitten.Infrastructure.Repositories;
using GymKitten.Infrastructure.Settings;
using GymKitten.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minio;

namespace GymKitten.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HttpContextAccessor & UserContext
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<GymkittenContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<GymkittenContext>());

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<GymkittenContext>());

        // Repositories
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // MinIO Settings & Client
        var minioSection = configuration.GetSection(MinioSettings.SectionName);
        services.Configure<MinioSettings>(minioSection);

        services.AddSingleton<IMinioClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;
            var client = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey);

            if (settings.UseSSL)
            {
                client = client.WithSSL();
            }

            return client.Build();
        });

        services.AddScoped<IStorageService, MinioStorageService>();

        // JWT Settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSettings);

        // Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // Auth services
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // OTP & Email services (stub implementations)
        services.AddSingleton<IOtpGenerator, OtpGenerator>();
        services.AddSingleton<IRedisOtpStore, InMemoryOtpStore>();
        services.AddSingleton<IEmailJobService, StubEmailJobService>();

        // Register MediatR notification handlers from Infrastructure assembly
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
