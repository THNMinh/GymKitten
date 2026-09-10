# BẢN THIẾT KẾ & HƯỚNG DẪN BƯNG BACKGROUND JOBS HANGFIRE (HANGFIRE BACKGROUND JOBS BLUEPRINT)

Tài liệu này hướng dẫn chi tiết cách cấu hình và triển khai **Hệ thống Xử lý Tác vụ Ngầm (Background Jobs)** sử dụng **Hangfire (PostgreSQL Storage)** từ dự án **`Kickify_BE`** sang bất kỳ dự án Backend `.NET 8` nào.

---

## 🎯 1. TỔNG QUAN VỀ HANGFIRE TRONG KICKIFY

Hangfire giúp thực thi các tác vụ chạy ngầm, tác vụ hẹn giờ hoặc tác vụ định kỳ **bất đồng bộ**, không làm treo HTTP Request của người dùng, tự động thử lại khi thất bại (Auto Retry) và lưu trữ trạng thái bền vững trong Database PostgreSQL.

### 4 Dạng Job được sử dụng trong dự án:
1. **Fire-and-Forget Jobs**: Chạy ngầm 1 lần duy nhất ngay lập tức (Ví dụ: Gửi Email OTP, Reset Password).
2. **Delayed / Scheduled Jobs**: Hẹn giờ chạy sau N phút/giờ (Ví dụ: Tự động đóng phòng sau 15 phút, Hủy kèo quá hạn). Có khả năng **Hủy Job (Cancel)** hoặc **Dời lịch (Reschedule)**.
3. **Recurring Jobs**: Chạy định kỳ theo biểu thức Cron (Ví dụ: Cập nhật Bảng xếp hạng 5 phút/lần, dọn dẹp Log hệ thống).
4. **Batch Background Service**: Gom nhóm dữ liệu ghi theo lô (Batch Insert) bằng `Channel<T>` để tối ưu I/O Database.

---

## ⚙️ 2. CẤU HÌNH & DEPENDENCY INJECTION (STEP-BY-STEP SETUP)

### Bước 1: Cài đặt NuGet Packages

Thêm các thư viện Hangfire vào dự án **Infrastructure** và **Api**:

```bash
# In Infrastructure:
dotnet add YourProject.Infrastructure package Hangfire.Core
dotnet add YourProject.Infrastructure package Hangfire.PostgreSql

# In Api:
dotnet add YourProject.Api package Hangfire.AspNetCore
```

---

### Bước 2: Cấu hình `appsettings.json`

Thêm thông tin tài khoản đăng nhập Dashboard Hangfire:

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=your_db;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Hangfire": {
    "Username": "admin",
    "Password": "YOUR_HANGFIRE_PASSWORD"
  }
}
```

---

### Bước 3: Đăng ký Services trong `Infrastructure/DependencyInjection.cs`

Tạo extension method `AddHangfireServices` lưu trữ trạng thái Job trong PostgreSQL (tạo schema `hangfire` riêng để tránh lẫn lộn với các bảng chính):

```csharp
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddHangfireServices(configuration)
            .AddBackgroundJobs();

    private static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        // 1. Cấu hình Hangfire sử dụng PostgreSQL Storage
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    SchemaName = "hangfire", // Schema riêng cho Hangfire trong PostgreSQL
                    QueuePollInterval = TimeSpan.FromSeconds(15)
                }));

        // 2. Kích hoạt Worker Server xử lý Job
        services.AddHangfireServer();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddScoped<IEmailJobService, EmailJobService>();
        services.AddScoped<IRoomAutoCloseService, RoomAutoCloseService>();
        services.AddHostedService<JobSchedulerStartupService>(); // Kích hoạt Recurring Jobs khi App start
        return services;
    }
}
```

---

### Bước 4: Bảo mật Dashboard bằng Basic Auth (`HangfireAuthorizationFilter.cs`)

Tạo file tại `YourProject.Api/Hangfire/HangfireAuthorizationFilter.cs`. Filter này yêu cầu nhập Username/Password và lưu `HttpOnly Auth Cookie` trong 8 giờ:

```csharp
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace YourProject.Api.Hangfire;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireAuthorizationFilter(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // 1. Kiểm tra Cookie đã xác thực trước đó chưa
        if (httpContext.Request.Cookies.TryGetValue("HangfireAuth", out var authCookie))
        {
            if (ValidateAuthCookie(authCookie)) return true;
        }

        // 2. Kiểm tra Header Basic Authentication
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Basic "))
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);

            if (parts.Length == 2 && parts[0] == _username && parts[1] == _password)
            {
                // Lưu Auth Cookie
                httpContext.Response.Cookies.Append("HangfireAuth", GenerateAuthCookie(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
                return true;
            }
        }

        // Trả về HTTP 401 bật hộp thoại nhập Password của trình duyệt
        httpContext.Response.StatusCode = 401;
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
        return false;
    }

    private string GenerateAuthCookie()
    {
        var data = $"{_username}:{DateTime.UtcNow:O}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }

    private bool ValidateAuthCookie(string cookie)
    {
        try
        {
            var data = Encoding.UTF8.GetString(Convert.FromBase64String(cookie));
            var parts = data.Split(':', 2);
            if (parts.Length == 2 && parts[0] == _username)
            {
                if (DateTime.TryParse(parts[1], out var createdAt))
                {
                    return DateTime.UtcNow.Subtract(createdAt).TotalHours < 8;
                }
            }
        }
        catch { }
        return false;
    }
}
```

---

### Bước 5: Bật Dashboard UI trong `Program.cs`

Đấu nối Dashboard tại đường dẫn `/hangfire` với bộ bảo mật Basic Auth:

```csharp
using Hangfire;
using YourProject.Api.Hangfire;

var app = builder.Build();

// ... Các middleware khác ...

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] 
    { 
        new HangfireAuthorizationFilter(
            builder.Configuration["Hangfire:Username"]!, 
            builder.Configuration["Hangfire:Password"]!) 
    },
    DashboardTitle = "YourProject - Hangfire Dashboard",
    DisplayStorageConnectionString = false
});

app.Run();
```

---

## 💻 3. TRIỂN KHAI 4 DẠNG JOB THỰC TẾ (CODE TEMPLATES)

### 3.1. Dạng 1: Fire-and-Forget Job (Gửi Mail OTP)

Đưa tác vụ gửi email vào hàng chờ background để API trả về kết quả 201 Created ngay lập tức cho client.

#### Interface (`IEmailJobService.cs`):
```csharp
namespace YourProject.Application.Abstractions.Jobs;

public interface IEmailJobService
{
    void EnqueueSendOtpEmail(string toEmail, string otp);
}
```

#### Implementation (`EmailJobService.cs`):
```csharp
using Hangfire;
using YourProject.Application.Abstractions.Jobs;
using YourProject.Application.Abstractions.Services;

namespace YourProject.Infrastructure.Jobs;

public class EmailJobService : IEmailJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public EmailJobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueSendOtpEmail(string toEmail, string otp)
    {
        // Enqueue: Chạy ngầm 1 lần duy nhất ngay lập tức
        _backgroundJobClient.Enqueue<IMailService>(mail => mail.SendOtpAsync(toEmail, otp));
    }
}
```

---

### 3.2. Dạng 2: Delayed / Scheduled Job (Hẹn giờ & Hủy Job)

Ví dụ bài toán **Tự động đóng phòng / Hủy đơn sau N phút** nếu không đủ người. Cần lưu `JobId` vào Entity DB để có thể **Hủy** hoặc **Dời lịch (Reschedule)** sau này.

#### Interface (`IRoomAutoCloseService.cs`):
```csharp
namespace YourProject.Application.Abstractions.Jobs;

public interface IRoomAutoCloseService
{
    void ScheduleAutoClose(Guid roomId, TimeSpan delay);
    void CancelAutoClose(string? jobId);
    void RescheduleAutoClose(Guid roomId, string? oldJobId, TimeSpan delay);
}
```

#### Implementation (`RoomAutoCloseService.cs`):
```csharp
using Hangfire;
using YourProject.Application.Abstractions.Jobs;
using YourProject.Application.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace YourProject.Infrastructure.Jobs;

public class RoomAutoCloseService : IRoomAutoCloseService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RoomAutoCloseService(
        IBackgroundJobClient backgroundJobClient,
        IServiceScopeFactory serviceScopeFactory)
    {
        _backgroundJobClient = backgroundJobClient;
        _serviceScopeFactory = serviceScopeFactory;
    }

    // 1. Hẹn giờ đóng phòng sau thời gian 'delay'
    public void ScheduleAutoClose(Guid roomId, TimeSpan delay)
    {
        // Schedule: Hẹn giờ thực thi
        string jobId = _backgroundJobClient.Schedule(
            () => CloseRoomAsync(roomId),
            delay);

        // Lưu JobId vào Database để quản lý (Hủy hoặc Dời lịch)
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = repository.GetByIdAsync(roomId).GetAwaiter().GetResult();
        if (room != null)
        {
            room.AutoCloseJobId = jobId; // Cột AutoCloseJobId trong DB
            repository.Update(room);
            unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
        }
    }

    // 2. Hủy Job ngầm nếu người dùng chủ động đóng hoặc gia hạn
    public void CancelAutoClose(string? jobId)
    {
        if (string.IsNullOrEmpty(jobId)) return;
        _backgroundJobClient.Delete(jobId); // Delete xóa Job khỏi queue của Hangfire
    }

    // 3. Dời lịch Job
    public void RescheduleAutoClose(Guid roomId, string? oldJobId, TimeSpan delay)
    {
        CancelAutoClose(oldJobId);
        ScheduleAutoClose(roomId, delay);
    }

    // Phương thức thực thi logic khi Job chạy
    public async Task CloseRoomAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await repository.GetByIdAsync(roomId);
        if (room != null && room.Status == RoomStatus.Open)
        {
            room.Status = RoomStatus.Cancelled;
            repository.Update(room);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
```

---

### 3.3. Dạng 3: Recurring Cron Job (Chạy định kỳ tự động khi Start App)

Sử dụng `IHostedService` để khởi tạo danh sách Job chạy định kỳ khi ứng dụng vừa khởi động.

#### Implementation (`JobSchedulerStartupService.cs`):
```csharp
using Hangfire;
using Microsoft.Extensions.Hosting;
using YourProject.Application.Abstractions.Services;

namespace YourProject.Infrastructure.Jobs;

public class JobSchedulerStartupService : IHostedService
{
    private readonly IRecurringJobManager _recurringJobManager;

    public JobSchedulerStartupService(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 1. Cập nhật Bảng xếp hạng mỗi 5 phút/lần
        _recurringJobManager.AddOrUpdate<ILeaderboardUpdateService>(
            "leaderboard-update-job",
            service => service.UpdateLeaderboardAsync(),
            Cron.MinuteInterval(5));

        // 2. Dọn dẹp Log quá hạn mỗi 3 tháng 1 lần
        _recurringJobManager.AddOrUpdate<ISystemLogCleanupService>(
            "system-log-cleanup-job",
            service => service.CleanupOldLogsAsync(),
            "0 0 1 */3 *"); // Cron format: 0h ngày 1 của mỗi 3 tháng

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

---

## ⚠️ 4. QUY TẮC CẦN NHỚ KHI VIẾT HANGFIRE JOB (CRITICAL RULES)

> [!IMPORTANT]
> **1. Quy tắc Dependency Scope (IServiceScopeFactory):**  
> Hangfire Job chạy ở một Thread riêng độc lập với HTTP Request. Do đó **KHÔNG ĐƯỢC INJECT** `DbContext` hay Scoped Service trực tiếp vào constructor của Job Service.  
> **Giải pháp**: Luôn dùng `IServiceScopeFactory.CreateScope()` bên trong hàm thực thi của Job để tạo Scope DB riêng biệt và tự dọn dẹp bộ nhớ khi Job chạy xong.

> [!TIP]
> **2. Quản lý JobId trong Entity DB:**  
> Khi tạo Delayed Job (`Schedule`), luôn lưu `JobId` (`string`) vào cột tương ứng trong Entity Database. Việc này giúp bạn có thể gọi `_backgroundJobClient.Delete(jobId)` để Hủy Job nếu người dùng hủy thao tác.

---

## ✅ 5. CHECKLIST 6 BƯỚC MANG HANGFIRE SANG DỰ ÁN MỚI

```
□ 1. [NuGet]          Cài Hangfire.Core + Hangfire.PostgreSql (Infra) và Hangfire.AspNetCore (Api).
□ 2. [appsettings]    Cấu hình Hangfire:Username và Hangfire:Password.
□ 3. [Infrastructure] Đăng ký AddHangfire() với schema "hangfire" và AddHangfireServer().
□ 4. [Api Filter]     Tạo HangfireAuthorizationFilter.cs để bảo mật Basic Auth cho Dashboard.
□ 5. [Program.cs]     Đăng ký app.UseHangfireDashboard("/hangfire", ...).
□ 6. [Jobs]           Triển khai IEmailJobService (Enqueue) & IRoomAutoCloseService (Schedule/Delete).
```

---

*Tài liệu này được trích xuất và chuẩn hóa từ hệ thống xử lý tác vụ ngầm của Kickify_BE.*
