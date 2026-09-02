using System.Text;
using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Jobs;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Application.Abstractions.Storage;
using GymKitten.Infrastructure.Auth;
using GymKitten.Infrastructure.Jobs;
using GymKitten.Infrastructure.Payment;
using GymKitten.Infrastructure.Payment.MoMo;
using GymKitten.Infrastructure.Repositories;
using GymKitten.Infrastructure.Settings;
using GymKitten.Infrastructure.Storage;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minio;
using VNPAY;
using VNPAY.Extensions;

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
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderTrackingRepository, OrderTrackingRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        // Hangfire PostgreSQL Setup
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(connectionString),
                    new PostgreSqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true,
                        SchemaName = "hangfire",
                        QueuePollInterval = TimeSpan.FromSeconds(15)
                    }));

            services.AddHangfireServer();
        }

        services.AddScoped<IOrderAutoCancelService, OrderAutoCancelService>();

        // VNPay Setup
        var vnpayConfig = configuration.GetSection("VNPAY");
        services.AddVnpayClient(config =>
        {
            config.TmnCode = vnpayConfig["TmnCode"] ?? "CGXZ858Z";
            config.HashSecret = vnpayConfig["HashSecret"] ?? "NRA35K51WII3FWT1IARU2GOM647ZOMN0";
            config.BaseUrl = vnpayConfig["BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            config.CallbackUrl = vnpayConfig["CallbackUrl"] ?? "https://localhost:7191/api/payment/vnpay-callback";
            config.Version = vnpayConfig["Version"] ?? "2.1.0";
            config.OrderType = vnpayConfig["OrderType"] ?? "other";
        });

        services.AddScoped<IVnPayService, VnPayService>();

        // MoMo Setup
        var momoSection = configuration.GetSection(MomoOptionModel.SectionName);
        services.Configure<MomoOptionModel>(momoSection);
        services.AddHttpClient<IMomoService, MomoService>();

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

        // OTP & Email services
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
