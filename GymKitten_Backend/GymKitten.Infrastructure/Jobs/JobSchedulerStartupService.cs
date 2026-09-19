using GymKitten.Application.Abstractions.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.Jobs;

public class JobSchedulerStartupService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<JobSchedulerStartupService> _logger;

    public JobSchedulerStartupService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<JobSchedulerStartupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 [JobSchedulerStartupService] Initializing Hangfire recurring jobs scheduler...");

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();

            // 1. Lên lịch tự động quét và vô hiệu hóa mã giảm giá hết hạn hàng ngày
            var autoExpireCouponsJob = scope.ServiceProvider.GetRequiredService<IAutoExpireCouponsJob>();
            autoExpireCouponsJob.ScheduleAutoExpireCoupons();

            // Sau này nếu có thêm các Recurring Jobs khác (như SystemLogCleanupService), chỉ cần gọi thêm tại đây:
            // var systemLogCleanupService = scope.ServiceProvider.GetRequiredService<ISystemLogCleanupService>();
            // systemLogCleanupService.ScheduleSystemLogCleanup();

            _logger.LogInformation("✅ [JobSchedulerStartupService] All recurring jobs registered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [JobSchedulerStartupService] Failed to schedule recurring jobs on startup.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
