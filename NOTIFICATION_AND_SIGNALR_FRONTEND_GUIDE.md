# 🔔 HƯỚNG DẪN TÍCH HỢP CHUÔNG THÔNG BÁO & SIGNALR REAL-TIME (FRONTEND INTEGRATION GUIDE)

> **Dành cho Frontend Developers (React, Vue, Next.js, Flutter, Mobile)**  
> Tài liệu này mô tả chi tiết:
> 1. Những thay đổi nghiệp vụ của API **Update Order Status** (sự kiện phát sinh tự động).
> 2. Danh sách **3 Client APIs** quản lý danh sách thông báo & đánh dấu đã đọc.
> 3. Hướng dẫn toàn diện cách **kết nối, lắng nghe và quản lý vòng đời SignalR WebSockets** trên giao diện Frontend.

---

## 📌 1. TỔNG QUAN HỆ THỐNG THÔNG BÁO REAL-TIME (ARCHITECTURE OVERVIEW)

Trước đây, khi Admin cập nhật trạng thái đơn hàng hoặc hệ thống có thông báo mới, khách hàng phải **F5 (Reload lại trang)** mới thấy sự thay đổi.

Với kiến trúc mới kết hợp **Domain Events + SignalR Hub**, hệ thống hoạt động theo luồng Event-Driven tức thời:

```
[1. Admin Update Order Status]
             │
             ▼
[2. DB Order updated + SaveChangesAsync()]
             │
             ▼
[3. Bắn OrderStatusChangedDomainEvent]
             │
             ├───> [4. Lưu bản ghi Notification vào PostgreSQL (IsRead = false)]
             │
             └───> [5. SignalR Hub gửi socket tới group "user_{userId}"]
                                 │
                                 ▼
                     [6. Web Client nhận event "ReceiveNotification"]
                                 │
                                 ├──> Quả chuông nảy số đỏ (+1 Unread)
                                 ├──> Hiển thị Toast Popup góc màn hình
                                 └──> Thêm thông báo mới vào đầu Dropdown
```

---

## 🔄 2. CHI TIẾT THAY ĐỔI CỦA API CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (`OrderStatusChange`)

### Endpoint: `PUT /api/admin/orders/{orderId}/status`
*(Chỉ dành cho Admin)*

### 2.1. Trước và sau khi nâng cấp:
| Đặc tính | Trước đây | Sau khi nâng cấp |
| :--- | :--- | :--- |
| **Cập nhật dữ liệu Order** | Đổi status, trừ/hoàn kho, ghi `Ordertrackinghistory` | Giữ nguyên toàn bộ logic cũ |
| **Thông báo khách hàng** | Không có thông báo in-app | Tự động sinh `Notification` trong DB |
| **Phản hồi thời gian thực** | Khách hàng phải F5 trang | Bắn WebSocket tức thì xuống trình duyệt khách |

### 2.2. Nội dung thông báo tự động sinh ra cho khách hàng:
Khi Admin chuyển trạng thái đơn hàng, hệ thống tự động sinh ra thông báo với format:
* **Tiêu đề (`title`)**: `Cập nhật đơn hàng #{orderCode}` (Ví dụ: `Cập nhật đơn hàng #GK-260831-891`)
* **Nội dung (`content`)**: `Đơn hàng #{orderCode} của bạn đã đổi trạng thái thành: {newStatus}`
* **Loại (`type`)**: `"Order"`
* **Đường dẫn đích (`targetUrl`)**: `/orders/{orderId}` (Dùng để điều hướng khi khách click vào thông báo)
* **Trạng thái (`isRead`)**: `false`

---

## 🛒 3. DANH SÁCH 3 CLIENT APIS CHO QUẢ CHUÔNG THÔNG BÁO

Tất cả các API dưới đây đều yêu cầu Header:
```http
Authorization: Bearer <YOUR_ACCESS_TOKEN>
```

---

### 3.1. Lấy danh sách thông báo của tôi (`GET /api/notifications`)
* **Mục đích**: Gọi khi người dùng mở trang web (hoặc khi click mở Dropdown quả chuông).
* **Query Parameters**:
  - `page` *(int, Optional, Default: `1`)*: Số trang cần lấy.
  - `pageSize` *(int, Optional, Default: `10`)*: Số lượng thông báo mỗi trang.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "value": {
    "notifications": [
      {
        "notificationId": "8f3b2075-8120-41da-a7a5-d8ca2601bb99",
        "title": "Cập nhật đơn hàng #GK-260831-891",
        "content": "Đơn hàng #GK-260831-891 của bạn đã đổi trạng thái thành: Shipped",
        "type": "Order",
        "isRead": false,
        "targetUrl": "/orders/b0000000-0000-0000-0000-000000000099",
        "readAt": null,
        "createdAt": "2026-09-04T12:45:10.512Z"
      },
      {
        "notificationId": "1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b",
        "title": "Mã giảm giá mới dành riêng cho bạn!",
        "content": "Nhập mã GYM20 để được giảm 20% cho đơn hàng tiếp theo.",
        "type": "Promotion",
        "isRead": true,
        "targetUrl": "/coupons",
        "readAt": "2026-09-04T10:15:00Z",
        "createdAt": "2026-09-04T09:00:00Z"
      }
    ],
    "unreadCount": 1,
    "totalCount": 2,
    "page": 1,
    "pageSize": 10
  }
}
```

> [!TIP]
> **Hiển thị Badge Quả Chuông**:
> Dùng trực tiếp trường `value.unreadCount` để hiển thị chấm đỏ số lượng trên icon chuông:
> - Nếu `unreadCount > 0`: Hiển thị Badge (Ví dụ: `1`, `5`, `99+`).
> - Nếu `unreadCount === 0`: Ẩn Badge.

---

### 3.2. Đánh dấu 1 thông báo là đã đọc (`PUT /api/notifications/{id}/read`)
* **Mục đích**: Gọi khi người dùng click vào 1 dòng thông báo cụ thể trên Dropdown.
* **URL Parameter**:
  - `id` (`Guid` - Required): ID của thông báo (`notificationId`).
* **Request Body**: Không có.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "value": null
}
```

#### 🔴 Response Thất bại (`404 Not Found`):
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "The requested notification was not found.",
  "extensions": {
    "code": "Notification.NotFound"
  }
}
```

#### 💡 Xử lý State trên Frontend:
```javascript
// Cập nhật lạc quan (Optimistic Update) trong State:
setNotifications(prev => prev.map(item => 
  item.notificationId === id ? { ...item, isRead: true } : item
));
setUnreadCount(prev => Math.max(0, prev - 1));

// Điều hướng người dùng tới trang đích của thông báo:
navigate(notification.targetUrl);
```

---

### 3.3. Đánh dấu tất cả là đã đọc (`PUT /api/notifications/read-all`)
* **Mục đích**: Gọi khi người dùng nhấn nút **"Đánh dấu tất cả đã đọc"** trên Header của Dropdown thông báo.
* **Request Body**: Không có.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "value": null
}
```

#### 💡 Xử lý State trên Frontend:
```javascript
// Đưa toàn bộ danh sách hiện tại thành đã đọc và reset unreadCount = 0
setNotifications(prev => prev.map(item => ({ ...item, isRead: true })));
setUnreadCount(0);
```

---

## 📡 4. HƯỚNG DẪN CẤU HÌNH & TÍCH HỢP SIGNALR CLIENT

### 4.1. Cài đặt thư viện chính thức từ Microsoft
```bash
npm install @microsoft/signalr
```

---

### 4.2. Cơ chế Xác thực WebSocket qua Query String
Trình duyệt chuẩn **không cho phép** truyền HTTP Header `Authorization` trong quá trình bắt tay (Handshake) WebSocket.  
Do đó, Backend GymKitten đã được cấu hình tự động trích xuất token từ Query Param:
```
wss://localhost:7191/hubs/notification?access_token=YOUR_JWT_ACCESS_TOKEN
```

Thư viện `@microsoft/signalr` hỗ trợ việc này tự động thông qua option `accessTokenFactory`.

---

### 4.3. Viết Service Quản Lý SignalR (`services/signalrService.ts`)

```typescript
import * as signalR from "@microsoft/signalr";

export interface NotificationPayload {
  notificationId: string;
  title: string;
  content: string;
  type: "Order" | "Promotion" | "System";
  targetUrl: string;
  createdAt: string;
  isRead: boolean;
}

class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;

  public async startConnection(
    getToken: () => string | null,
    onNotificationReceived: (notification: NotificationPayload) => void
  ): Promise<void> {
    // Tránh khởi tạo kết nối trùng lặp nếu đã kết nối
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const hubUrl = `${import.meta.env.VITE_API_URL || "https://localhost:7191"}/hubs/notification`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => getToken() ?? "",
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        skipNegotiation: false
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Tự động reconnect sau 0s, 2s, 5s, 10s, 30s
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // 🔔 Lắng nghe sự kiện từ Server
    this.hubConnection.on("ReceiveNotification", (notification: NotificationPayload) => {
      console.log("📥 [SignalR Received]", notification);
      onNotificationReceived(notification);
    });

    // Bắt sự kiện khi reconnect
    this.hubConnection.onreconnecting((error) => {
      console.warn("⚠️ [SignalR Reconnecting] Mất kết nối, đang thử lại...", error);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log("✅ [SignalR Reconnected] Đã kết nối lại thành công, ID:", connectionId);
    });

    this.hubConnection.onclose((error) => {
      console.error("❌ [SignalR Closed] Kết nối đã đóng hoàn toàn:", error);
    });

    try {
      await this.hubConnection.start();
      console.log("🚀 [SignalR Connected] Kết nối WebSocket thông báo thành công!");
    } catch (error) {
      console.error("❌ [SignalR Error] Kết nối thất bại:", error);
      // Thử kết nối lại sau 5s nếu lỗi khởi tạo lần đầu
      setTimeout(() => this.startConnection(getToken, onNotificationReceived), 5000);
    }
  }

  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      this.hubConnection.off("ReceiveNotification");
      await this.hubConnection.stop();
      this.hubConnection = null;
      console.log("🛑 [SignalR Disconnected] Đã ngắt kết nối WebSocket.");
    }
  }
}

export const signalRService = new SignalRService();
```

---

### 4.4. Custom Hook React Hoàn Chỉnh (`hooks/useNotifications.ts`)

```typescript
import { useState, useEffect, useCallback } from "react";
import { signalRService, NotificationPayload } from "../services/signalrService";
import axios from "axios";
import { toast } from "react-toastify"; // Hoặc thư viện Toast bạn đang dùng

export interface NotificationItem extends NotificationPayload {
  readAt?: string | null;
}

export const useNotifications = (accessToken: string | null) => {
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);

  // 1. Fetch danh sách ban đầu từ REST API
  const fetchNotifications = useCallback(async () => {
    if (!accessToken) return;
    try {
      setLoading(true);
      const res = await axios.get("/api/notifications?page=1&pageSize=20", {
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      if (res.data.isSuccess) {
        setNotifications(res.data.value.notifications);
        setUnreadCount(res.data.value.unreadCount);
      }
    } catch (err) {
      console.error("Lỗi khi tải thông báo:", err);
    } finally {
      setLoading(false);
    }
  }, [accessToken]);

  // 2. Handler khi nhận thông báo Real-time từ SignalR
  const handleIncomingNotification = useCallback((incoming: NotificationPayload) => {
    // Cập nhật State: Tăng unreadCount & thêm vào đầu danh sách
    setUnreadCount(prev => prev + 1);
    setNotifications(prev => [incoming, ...prev]);

    // Bắn Toast Popup hiển thị ngay lập tức
    toast.info(`🔔 ${incoming.title}: ${incoming.content}`, {
      onClick: () => {
        // Chuyển hướng khi click Toast
        window.location.href = incoming.targetUrl;
      }
    });

    // Phát âm thanh báo hiệu nhẹ (Tùy chọn)
    try {
      const audio = new Audio("/sounds/notification.mp3");
      audio.play();
    } catch {}
  }, []);

  // 3. Quản lý vòng đời kết nối SignalR
  useEffect(() => {
    if (!accessToken) {
      signalRService.stopConnection();
      return;
    }

    // Tải dữ liệu ban đầu
    fetchNotifications();

    // Kết nối SignalR Hub
    signalRService.startConnection(() => accessToken, handleIncomingNotification);

    // Cleanup khi unmount hoặc khi logout (accessToken = null)
    return () => {
      signalRService.stopConnection();
    };
  }, [accessToken, fetchNotifications, handleIncomingNotification]);

  // 4. Hàm đánh dấu 1 thông báo đã đọc
  const markAsRead = async (notificationId: string, targetUrl?: string) => {
    try {
      // Optimistic update
      setNotifications(prev =>
        prev.map(n => n.notificationId === notificationId ? { ...n, isRead: true } : n)
      );
      setUnreadCount(prev => Math.max(0, prev - 1));

      await axios.put(`/api/notifications/${notificationId}/read`, null, {
        headers: { Authorization: `Bearer ${accessToken}` }
      });

      if (targetUrl) {
        window.location.href = targetUrl;
      }
    } catch (err) {
      console.error("Lỗi khi đánh dấu đã đọc:", err);
    }
  };

  // 5. Hàm đánh dấu tất cả đã đọc
  const markAllAsRead = async () => {
    try {
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
      setUnreadCount(0);

      await axios.put("/api/notifications/read-all", null, {
        headers: { Authorization: `Bearer ${accessToken}` }
      });
    } catch (err) {
      console.error("Lỗi khi đánh dấu tất cả đã đọc:", err);
    }
  };

  return {
    notifications,
    unreadCount,
    loading,
    refresh: fetchNotifications,
    markAsRead,
    markAllAsRead
  };
};
```

---

### 4.5. Component Quả Chuông Giao Diện (`components/NotificationBell.tsx`)

```tsx
import React, { useState } from "react";
import { useNotifications } from "../hooks/useNotifications";

interface Props {
  accessToken: string | null;
}

export const NotificationBell: React.FC<Props> = ({ accessToken }) => {
  const [isOpen, setIsOpen] = useState(false);
  const { notifications, unreadCount, markAsRead, markAllAsRead } = useNotifications(accessToken);

  return (
    <div className="relative inline-block text-left">
      {/* Nút bấm Quả chuông */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-gray-700 hover:text-black focus:outline-none"
      >
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>

        {/* Badge hiển thị số lượng chưa đọc */}
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 flex items-center justify-center min-w-[18px] h-[18px] px-1 text-xs font-bold text-white bg-red-600 rounded-full animate-pulse">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        )}
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute right-0 z-50 w-80 sm:w-96 mt-2 bg-white rounded-lg shadow-xl border border-gray-200">
          <div className="flex items-center justify-between p-3 border-b border-gray-100">
            <h3 className="font-semibold text-gray-800">Thông báo</h3>
            {unreadCount > 0 && (
              <button
                onClick={markAllAsRead}
                className="text-xs font-medium text-blue-600 hover:underline"
              >
                Đánh dấu tất cả đã đọc
              </button>
            )}
          </div>

          <div className="max-h-96 overflow-y-auto divide-y divide-gray-100">
            {notifications.length === 0 ? (
              <div className="p-4 text-center text-sm text-gray-500">
                Không có thông báo nào.
              </div>
            ) : (
              notifications.map((item) => (
                <div
                  key={item.notificationId}
                  onClick={() => markAsRead(item.notificationId, item.targetUrl)}
                  className={`p-3 cursor-pointer transition duration-150 ${
                    item.isRead ? "bg-white hover:bg-gray-50 opacity-75" : "bg-blue-50/50 hover:bg-blue-100/50 font-medium"
                  }`}
                >
                  <div className="flex justify-between items-start">
                    <p className="text-sm font-semibold text-gray-900">{item.title}</p>
                    {!item.isRead && (
                      <span className="w-2 h-2 mt-1.5 bg-blue-600 rounded-full shrink-0"></span>
                    )}
                  </div>
                  <p className="text-xs text-gray-600 mt-1 line-clamp-2">{item.content}</p>
                  <p className="text-[10px] text-gray-400 mt-1">
                    {new Date(item.createdAt).toLocaleString("vi-VN")}
                  </p>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
};
```

---

## ⚠️ 5. NHỮNG LƯU Ý QUAN TRỌNG KHI TÍCH HỢP FRONTEND (CRITICAL GOTCHAS)

1. **Khởi tạo đúng thời điểm**:
   - Chỉ gọi `startConnection()` khi người dùng **đã đăng nhập** và có `accessToken` hợp lệ.
   - Khi người dùng **Logout**, bắt buộc phải gọi `signalRService.stopConnection()` để đóng socket và xóa kết nối trên Server.

2. **Tránh Memory Leak & Duplicate Event Listeners**:
   - Không đăng ký `connection.on("ReceiveNotification")` nhiều lần trong các component con. Nên để ở 1 Hook hoặc Context Provider duy nhất ở tầng App.
   - Luôn dọn dẹp kết nối trong hàm `return () => { ... }` của `useEffect`.

3. **Cấu hình CORS & Credentials**:
   - Backend đã hỗ trợ `.AllowCredentials()`.
   - Nếu gọi API qua `fetch` hoặc `axios`, không cần thêm cấu hình đặc biệt cho SignalR vì thư viện `@microsoft/signalr` tự động đính kèm thông tin xác thực bắt tay.

4. **Trường hợp mất mạng / Mở lại tab**:
   - Option `withAutomaticReconnect([0, 2000, 5000, ...])` sẽ tự động kết nối lại khi người dùng kết nối lại mạng Internet.
   - Khi sự kiện `onreconnected` được gọi, bạn có thể gọi lại `fetchNotifications()` để đồng bộ lại danh sách thông báo phát sinh trong thời gian ngoại tuyến.
