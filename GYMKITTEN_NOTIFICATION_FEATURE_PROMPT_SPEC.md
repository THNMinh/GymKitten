# MASTER PROMPT & SPECIFICATION: HƯỚNG DẪN TRIỂN KHAI TÍNH NĂNG CHUÔNG THÔNG BÁO (NOTIFICATION & SIGNALR REALTIME)

> **MỤC ĐÍCH FILE NÀY**: File này đóng vai trò như một **Master Prompt / Specification hoàn chỉnh**. Bạn có thể dùng file này gửi cho AI / Developer khác để yêu cầu triển khai trọn gói **Feature 6: Chuông Thông báo Real-time (Notification Bell)** cho dự án GymKitten hoặc bất kỳ hệ thống `.NET (Clean Architecture)` nào khác.

---

## 🎯 1. YÊU CẦU TÍNH NĂNG (FEATURE SPECIFICATION)

### Feature 6: Chuông Thông báo (Notification)
- **Tác dụng**: Hiển thị quả chuông thông báo real-time/in-app cho khách hàng khi đơn hàng đổi trạng thái, có mã giảm giá mới hoặc thông báo hệ thống.
- **Công nghệ**: .NET 8 (Clean Architecture + CQRS với MediatR), EF Core, SignalR WebSockets Real-time, PostgreSQL.

### 🛒 Client APIs ([Authorize]):
1. `GET /api/notifications`: Lấy danh sách thông báo của tôi (có phân trang + trả về số lượng chưa đọc `unreadCount`).
2. `PUT /api/notifications/{id}/read`: Đánh dấu 1 thông báo là đã đọc (`Isread = true`, gán `Readat = UTC`).
3. `PUT /api/notifications/read-all`: Đánh dấu tất cả thông báo của tôi là đã đọc.

---

## 🗄️ 2. DATABASE SCHEMAS (GYMKITTEN ENTITIES)

Sử dụng nguyên bản 2 Entity hiện có của GymKitten:

### 2.1. Entity `Notification.cs`
```csharp
namespace GymKitten.Domain.Entities;

public partial class Notification
{
    public Guid Notificationid { get; set; }
    public Guid Userid { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Type { get; set; } = null!; // "Order", "Promotion", "System"
    public bool Isread { get; set; }
    public string? Targeturl { get; set; }
    public DateTime? Readat { get; set; }
    public DateTime Createdat { get; set; }
    public DateTime Updatedat { get; set; }
    public DateTime? Deletedat { get; set; }

    public virtual User User { get; set; } = null!;
}
```

### 2.2. Entity `User.cs`
```csharp
namespace GymKitten.Domain.Entities;

public partial class User
{
    public Guid Userid { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? Fcmtoken { get; set; }
    // ...
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
```

---

## 🏗️ 3. KIẾN TRÚC KĨ THUẬT (ARCHITECTURAL SETUP)

### 3.1. Sơ Đồ Luồng Tự Động Sinh Thông Báo (Event-Driven Flow)

```
[1. Action Kinh Doanh] ──> [2. Save DB Order] ──> [3. _publisher.Publish(OrderEvent)]
                                                                  │
                                                                  ▼
[6. SignalR Web Push] <── [5. Send NotificationHub] <── [4. EventHandler (MediatR)]
  (Quả chuông nảy số)       (Group: user_{Userid})                 │
                                                                   ▼
                                                         [Lưu DB Notification]
```

---

### 3.2. Cấu Hình SignalR In-App Bell

#### Step 1: `NotificationHub.cs` (`GymKitten.Api/Hubs/NotificationHub.cs`)
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GymKitten.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private Guid CurrentUserId => Guid.Parse(
        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new HubException("Unauthorized"));

    public override async Task OnConnectedAsync()
    {
        // Khi user mở Web, tự động gia nhập Group nhận tin cá nhân: user_{Userid}
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{CurrentUserId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{CurrentUserId}");
        await base.OnDisconnectedAsync(exception);
    }
}
```

#### Step 2: Service Abstraction & Implementation

📂 `GymKitten.Application/Abstractions/Services/INotificationHubService.cs`
```csharp
namespace GymKitten.Application.Abstractions.Services;

public interface INotificationHubService
{
    Task SendNotificationToUserAsync(Guid userId, object payload, CancellationToken cancellationToken = default);
}
```

📂 `GymKitten.Api/Services/NotificationHubService.cs`
```csharp
using GymKitten.Api.Hubs;
using GymKitten.Application.Abstractions.Services;
using Microsoft.AspNetCore.SignalR;

namespace GymKitten.Api.Services;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(Guid userId, object payload, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("ReceiveNotification", payload, cancellationToken);
    }
}
```

#### Step 3: Đăng ký Token Query String cho WebSockets & Endpoint Map (`Program.cs` & `DependencyInjection.cs`)

```csharp
// Trong Infrastructure DependencyInjection.cs cho JwtBearer:
o.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};

// Trong Program.cs:
app.MapHub<NotificationHub>("/hubs/notification");
```

---

## ⚡ 4. TÍCH HỢP TỰ ĐỘNG BẮN THÔNG BÁO VÀO APIS KINH DOANH

### 4.1. Khai báo Domain Events
📂 `GymKitten.Domain/Events/OrderStatusChangedDomainEvent.cs`
```csharp
using MediatR;

namespace GymKitten.Domain.Events;

public record OrderStatusChangedDomainEvent(
    Guid OrderId,
    Guid UserId,
    string OrderCode,
    string NewStatus,
    string StatusDescription
) : INotification;
```

---

### 4.2. Chèn `_publisher.Publish` trong Business Handler (`UpdateOrderStatusCommandHandler.cs`)
```csharp
// 1. Sau khi cập nhật Order và SaveChangesAsync thành công:
await _unitOfWork.SaveChangesAsync(cancellationToken);

// 2. Bắn Domain Event cho hệ thống thông báo
await _publisher.Publish(new OrderStatusChangedDomainEvent(
    order.Orderid,
    order.Userid,
    order.Ordercode ?? order.Orderid.ToString()[..8],
    order.Status,
    $"Đơn hàng #{order.Ordercode} của bạn đã đổi trạng thái thành: {order.Status}"
), cancellationToken);
```

---

### 4.3. Event Handler Xử Lý Lưu DB + Bắn SignalR Socket (`OrderStatusChangedEventHandler.cs`)
📂 `GymKitten.Application/Features/Notifications/Events/OrderStatusChangedEventHandler.cs`

```csharp
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Events;
using MediatR;

namespace GymKitten.Application.Features.Notifications.Events;

public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedDomainEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHubService _notificationHubService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderStatusChangedEventHandler(
        INotificationRepository notificationRepository,
        INotificationHubService notificationHubService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _notificationHubService = notificationHubService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderStatusChangedDomainEvent notificationEvent, CancellationToken cancellationToken)
    {
        // 1. LƯU BẢN GHI NOTIFICATION VÀO DB
        var notificationEntity = new Notification
        {
            Notificationid = Guid.NewGuid(),
            Userid = notificationEvent.UserId,
            Title = $"Cập nhật đơn hàng #{notificationEvent.OrderCode}",
            Content = notificationEvent.StatusDescription,
            Type = "Order",
            Isread = false,
            Targeturl = $"/orders/{notificationEvent.OrderId}",
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notificationEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 2. BẮN REALTIME ĐẾN SIGNALR HUB (Cập nhật quả chuông trên Web lập tức)
        var payload = new
        {
            NotificationId = notificationEntity.Notificationid,
            Title = notificationEntity.Title,
            Content = notificationEntity.Content,
            Type = notificationEntity.Type,
            TargetUrl = notificationEntity.Targeturl,
            CreatedAt = notificationEntity.Createdat,
            IsRead = false
        };

        await _notificationHubService.SendNotificationToUserAsync(notificationEvent.UserId, payload, cancellationToken);
    }
}
```

---

## 🛠️ 5. MÃ NGUỒN CHI TIẾT TỔNG HỢP 3 CLIENT APIS (MANAGE NOTIFICATIONS)

### 5.1. API 1: `GET /api/notifications` (Lấy danh sách & UnreadCount)

#### Query DTO & Response:
📂 `GymKitten.Application/Features/Notifications/Queries/GetMyNotifications/GetMyNotificationsQuery.cs`
```csharp
using MediatR;

namespace GymKitten.Application.Features.Notifications.Queries.GetMyNotifications;

public record NotificationDto(
    Guid NotificationId,
    string Title,
    string Content,
    string Type,
    bool IsRead,
    string? TargetUrl,
    DateTime? ReadAt,
    DateTime CreatedAt
);

public record GetMyNotificationsResponse(
    List<NotificationDto> Notifications,
    int UnreadCount,
    int TotalCount,
    int Page,
    int PageSize
);

public record GetMyNotificationsQuery(int Page = 1, int PageSize = 10) : IRequest<GetMyNotificationsResponse>;
```

#### Query Handler:
📂 `GymKitten.Application/Features/Notifications/Queries/GetMyNotifications/GetMyNotificationsQueryHandler.cs`
```csharp
using GymKitten.Application.Abstractions.Authentication;
using GymKitten.Application.Abstractions.Repositories;
using MediatR;

namespace GymKitten.Application.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, GetMyNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetMyNotificationsResponse> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var (items, totalCount) = await _notificationRepository.GetPagedByUserIdAsync(
            userId, request.Page, request.PageSize, cancellationToken);

        var unreadCount = await _notificationRepository.CountUnreadByUserIdAsync(userId, cancellationToken);

        var dtos = items.Select(n => new NotificationDto(
            n.Notificationid,
            n.Title,
            n.Content,
            n.Type,
            n.Isread,
            n.Targeturl,
            n.Readat,
            n.Createdat
        )).ToList();

        return new GetMyNotificationsResponse(dtos, unreadCount, totalCount, request.Page, request.PageSize);
    }
}
```

---

### 5.2. API 2: `PUT /api/notifications/{id}/read` (Đánh dấu 1 thông báo đã đọc)

#### Command DTO:
📂 `GymKitten.Application/Features/Notifications/Commands/MarkAsRead/MarkNotificationAsReadCommand.cs`
```csharp
using MediatR;

namespace GymKitten.Application.Features.Notifications.Commands.MarkAsRead;

public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<bool>;
```

#### Command Handler:
📂 `GymKitten.Application/Features/Notifications/Commands/MarkAsRead/MarkNotificationAsReadCommandHandler.cs`
```csharp
using GymKitten.Application.Abstractions.Authentication;
using GymKitten.Application.Abstractions.Persistence;
using GymKitten.Application.Abstractions.Repositories;
using MediatR;

namespace GymKitten.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var notification = await _notificationRepository.GetByIdAndUserIdAsync(request.NotificationId, userId, cancellationToken);

        if (notification == null) return false;

        if (!notification.Isread)
        {
            notification.Isread = true;
            notification.Readat = DateTime.UtcNow;
            notification.Updatedat = DateTime.UtcNow;

            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
```

---

### 5.3. API 3: `PUT /api/notifications/read-all` (Đánh dấu tất cả đã đọc)

#### Command DTO:
📂 `GymKitten.Application/Features/Notifications/Commands/MarkAllAsRead/MarkAllNotificationsAsReadCommand.cs`
```csharp
using MediatR;

namespace GymKitten.Application.Features.Notifications.Commands.MarkAllAsRead;

public record MarkAllNotificationsAsReadCommand : IRequest<bool>;
```

#### Command Handler:
📂 `GymKitten.Application/Features/Notifications/Commands/MarkAllAsRead/MarkAllNotificationsAsReadCommandHandler.cs`
```csharp
using GymKitten.Application.Abstractions.Authentication;
using GymKitten.Application.Abstractions.Persistence;
using GymKitten.Application.Abstractions.Repositories;
using MediatR;

namespace GymKitten.Application.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        await _notificationRepository.MarkAllAsReadByUserIdAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
```

---

### 5.4. Controller Interface (`NotificationsController.cs`)
📂 `GymKitten.Api/Controllers/NotificationsController.cs`

```csharp
using GymKitten.Application.Features.Notifications.Commands.MarkAllAsRead;
using GymKitten.Application.Features.Notifications.Commands.MarkAsRead;
using GymKitten.Application.Features.Notifications.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.Api.Controllers;

[Route("api/notifications")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET /api/notifications — Lấy danh sách thông báo của tôi (Kèm unreadCount)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyNotificationsQuery(page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// PUT /api/notifications/{id}/read — Đánh dấu 1 thông báo là đã đọc
    /// </summary>
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkNotificationAsReadCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        return success ? Ok() : NotFound(new { message = "Notification not found" });
    }

    /// <summary>
    /// PUT /api/notifications/read-all — Đánh dấu tất cả thông báo của tôi là đã đọc
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var command = new MarkAllNotificationsAsReadCommand();
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
}
```

---

## 📋 6. CHECKLIST KIỂM THỬ THÀNH CÔNG (VERIFICATION CHECKLIST)

```
[ ] 1. Mở SignalR Connection tại Web Client: wss://domain/hubs/notification?access_token=JWT_TOKEN.
[ ] 2. Gọi API Đổi trạng thái đơn hàng (VD: Pending -> Shipping).
[ ] 3. Kiểm tra DB: Bảng Notification xuất hiện 1 record mới với Isread = false.
[ ] 4. Kiểm tra Web Client: SignalR nhận event "ReceiveNotification" -> Quả chuông nảy số +1 Unread.
[ ] 5. Gọi GET /api/notifications: Trả về danh sách thông báo + unreadCount chính xác.
[ ] 6. Gọi PUT /api/notifications/{id}/read: Cột Isread đổi thành true, Readat được gán giờ UTC.
```
