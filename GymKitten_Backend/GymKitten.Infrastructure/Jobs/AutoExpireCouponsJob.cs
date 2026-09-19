using GymKitten.Application.Abstractions.Jobs;
using GymKitten.Application.Abstractions.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.Jobs;

public class AutoExpireCouponsJob : IAutoExpireCouponsJob
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AutoExpireCouponsJob> _logger;

    public AutoExpireCouponsJob(
        IRecurringJobManager recurringJobManager,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AutoExpireCouponsJob> logger)
    {
        _recurringJobManager = recurringJobManager;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public void ScheduleAutoExpireCoupons()
    {
        // Chạy mỗi ngày lúc 00:00 (midnight) UTC
        _recurringJobManager.AddOrUpdate(
            "auto-expire-coupons-job",
            () => ExecuteAsync(),
            Cron.Daily(0, 0), // 00:00 every day UTC
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        _logger.LogInformation("Scheduled auto-expire coupons job to run daily at 00:00 UTC");
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting auto-expire coupons job at {UtcTime}...", DateTime.UtcNow);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GymkittenContext>();
            var systemLogService = scope.ServiceProvider.GetService<ISystemLogService>();

            var now = DateTime.UtcNow;

            var couponsToExpire = await context.Coupons
                .Where(c => c.Isactive && c.Deletedat == null &&
                            (c.Enddate < now || (c.Usagelimit != null && c.Usedcount >= c.Usagelimit)))
                .ToListAsync();

            if (couponsToExpire.Count == 0)
            {
                _logger.LogInformation("Auto-expire coupons job completed: No coupons need deactivation.");
                return;
            }

            foreach (var coupon in couponsToExpire)
            {
                coupon.Isactive = false;
                coupon.Updatedat = now;
            }

            await context.SaveChangesAsync();

            var codes = string.Join(", ", couponsToExpire.Select(c => c.Code));
            _logger.LogInformation("Successfully deactivated {Count} expired/exhausted coupon(s): {Codes}", couponsToExpire.Count, codes);

            if (systemLogService != null)
            {
                await systemLogService.LogAsync(
                    "AutoExpireCoupons",
                    $"Auto-deactivated {couponsToExpire.Count} expired or exhausted coupon(s): [{codes}].",
                    "Information",
                    null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing auto-expire coupons job");
        }
    }
}
