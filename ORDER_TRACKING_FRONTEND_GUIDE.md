# 🚚 HƯỚNG DẪN TÍCH HỢP APIS QUẢN LÝ ĐƠN HÀNG & TRACKING HÀNH TRÌNH (ORDER & TRACKING TIMELINE)

> **Dành cho Frontend Developers**: Tài liệu hướng dẫn thiết kế giao diện **Quản lý Đơn hàng & Timeline Hành trình Giao hàng (Shopee/Lazada style)** cho cả Client và Admin.

---

## 📌 1. TỔNG QUAN HỆ THỐNG TRẠNG THÁI ĐƠN HÀNG (ORDER STATUS ENUM)

Hệ thống đơn hàng quản lý theo vòng đời chuẩn bao gồm 5 trạng thái (`CurrentStatus`):

| Enum Key | Chuỗi Trạng Thái | Mô tả chi tiết | Quyền Hủy Đơn của Khách | Ghi chú Tồn kho & Thanh toán |
| :--- | :--- | :--- | :---: | :--- |
| `Pending` | `"Pending"` | Đơn mới đặt, chờ xác nhận | **CÓ** (Bấm nút Hủy) | Đã giữ số lượng tạm thời trong kho (`QuantityReserved`). |
| `Processing` | `"Processing"` | Đóng gói / Chờ lấy hàng | KHÔNG | Admin đang chuẩn bị đóng gói hàng. |
| `Shipped` | `"Shipped"` | Đang giao hàng | KHÔNG | **Trừ tồn kho thực tế** (`QuantityOnHand` & `QuantityReserved`). |
| `Delivered` | `"Delivered"` | Đã giao hàng thành công | KHÔNG | Nếu là `COD`, tự động chuyển `PaymentStatus = "Paid"`. |
| `Cancelled` | `"Cancelled"` | Đã hủy đơn | KHÔNG | **Giải phóng tồn kho giữ** (`QuantityReserved`). |

---

## 🛒 2. CLIENT APIS (GIAO DIỆN KHÁCH HÀNG)

---

### 2.1 Lấy Danh Sách Đơn Hàng Của Tôi (`GET /api/orders/my-orders`)

* **Header**: `Authorization: Bearer <AccessToken>`
* **Query Parameters**:
  - `status` *(Optional)*: Lọc theo status: `"Pending"`, `"Processing"`, `"Shipped"`, `"Delivered"`, `"Cancelled"`.
  - `page` *(Default: 1)*: Trang hiện tại.
  - `pageSize` *(Default: 20)*: Số lượng đơn trên 1 trang.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "orderId": "b0000000-0000-0000-0000-000000000099",
        "orderCode": "GK-260831-891",
        "totalAmount": 1900000,
        "currentStatus": "Pending",
        "paymentMethod": "COD",
        "paymentStatus": "Unpaid",
        "createdAt": "2026-08-31T12:00:00Z",
        "totalItems": 2
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "error": null,
  "timestamp": "2026-08-31T12:55:00Z"
}
```

---

### 2.2 Xem Chi Tiết 1 Đơn Hàng (`GET /api/orders/{orderId}`)

* **Header**: `Authorization: Bearer <AccessToken>`
* **Path Parameter**: `{orderId}` - GUID của đơn hàng.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "orderId": "b0000000-0000-0000-0000-000000000099",
    "orderCode": "GK-260831-891",
    "userId": "a0000000-0000-0000-0000-000000000001",
    "shippingAddress": "123 Le Loi, Quan 1, TP.HCM",
    "subtotal": 1850000,
    "shippingFee": 50000,
    "discountAmount": 0,
    "totalAmount": 1900000,
    "currentStatus": "Pending",
    "paymentMethod": "COD",
    "paymentStatus": "Unpaid",
    "customerNote": "Giao giờ hành chính",
    "createdAt": "2026-08-31T12:00:00Z",
    "updatedAt": "2026-08-31T12:00:00Z",
    "items": [
      {
        "orderItemId": "c0000000-0000-0000-0000-000000000055",
        "variantId": "v0000000-0000-0000-0000-000000000011",
        "sku": "ONX-HOODIE-BLK-L",
        "productName": "Onyx Oversize Hoodie",
        "unitPrice": 950000,
        "quantity": 2,
        "totalPrice": 1900000
      }
    ]
  },
  "error": null,
  "timestamp": "2026-08-31T12:55:00Z"
}
```

---

### 2.3 Xem Lịch Sử Hành Trình Giao Hàng Timeline (`GET /api/orders/{orderId}/tracking`)

> 💡 **Dành cho Component Timeline Giao hàng**: Trả về mảng danh sách lịch sử sắp xếp theo thứ tự thời gian tăng dần từ lúc Đặt hàng -> Chờ xác nhận -> Đang đóng gói -> Đang giao -> Đã giao.

* **Header**: `Authorization: Bearer <AccessToken>`
* **Path Parameter**: `{orderId}` - GUID của đơn hàng.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": [
    {
      "trackingId": "t0000000-0000-0000-0000-000000000001",
      "orderId": "b0000000-0000-0000-0000-000000000099",
      "status": "Pending",
      "title": "Đơn hàng đã được khởi tạo",
      "description": "Khách hàng đã đặt đơn thành công.",
      "location": "Hệ thống GymKitten",
      "timestamp": "2026-08-31T12:00:00Z",
      "createdAt": "2026-08-31T12:00:00Z"
    },
    {
      "trackingId": "t0000000-0000-0000-0000-000000000002",
      "orderId": "b0000000-0000-0000-0000-000000000099",
      "status": "Shipped",
      "title": "Đơn hàng đang được vận chuyển",
      "description": "Bưu tá đã lấy hàng từ kho.",
      "location": "Trung tâm phân phối Bưu cục Tân Bình",
      "timestamp": "2026-08-31T14:30:00Z",
      "createdAt": "2026-08-31T14:30:00Z"
    }
  ],
  "error": null,
  "timestamp": "2026-08-31T12:55:00Z"
}
```

---

### 2.4 Khách Hàng Tự Bấm Hủy Đơn (`PUT /api/orders/{orderId}/cancel`)

> ⚠️ **Lưu ý**: Khách hàng chỉ hủy được khi `currentStatus == "Pending"`. Nếu đơn đã ở trạng thái khác (`Processing`, `Shipped`), Backend sẽ trả về lỗi `Order.CannotCancelNonPendingOrder`.

* **Header**: `Authorization: Bearer <AccessToken>`
* **Path Parameter**: `{orderId}` - GUID của đơn hàng.

#### 🟢 Response khi thành công (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "orderId": "b0000000-0000-0000-0000-000000000099",
    "status": "Cancelled",
    "message": "Order cancelled successfully."
  },
  "error": null,
  "timestamp": "2026-08-31T12:55:00Z"
}
```

#### 🔴 Response khi không hợp lệ (`400 Bad Request`):
```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Only pending orders can be cancelled by the customer.",
  "extensions": {
    "code": "Order.CannotCancelNonPendingOrder"
  }
}
```

---

## 🛡️ 3. ADMIN APIS (QUẢN TRỊ VIÊN)

---

### 3.1 Quản Lý Danh Sách Tất Cả Đơn Hàng (`GET /api/admin/orders`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Bắt buộc Role `Admin`)
* **Query Parameters**:
  - `orderCode` *(Optional)*: Tìm kiếm theo mã đơn (vd: `"GK-260831"`).
  - `status` *(Optional)*: Lọc theo trạng thái đơn.
  - `startDate` / `EndDate` *(Optional)*: Lọc theo khoảng ngày (ISO String `2026-08-01T00:00:00Z`).
  - `page` / `pageSize` *(Default: 1 / 20)*.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "orderId": "b0000000-0000-0000-0000-000000000099",
        "orderCode": "GK-260831-891",
        "customerEmail": "user@example.com",
        "totalAmount": 1900000,
        "currentStatus": "Processing",
        "paymentMethod": "VNPAY",
        "paymentStatus": "Paid",
        "createdAt": "2026-08-31T12:00:00Z",
        "totalItems": 2
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "error": null,
  "timestamp": "2026-08-31T12:55:00Z"
}
```

---

### 3.2 Admin Cập Nhật Trạng Thái Đơn & Chèn Timeline (`PUT /api/admin/orders/{orderId}/status`)

> 💡 **Tác dụng**: Cập nhật trạng thái đơn sang `Processing`, `Shipped`, `Delivered`, hoặc `Cancelled` đồng thời ghi thêm thông tin ghi chú vị trí bưu cục để chèn trực tiếp vào Timeline giao hàng.

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Bắt buộc Role `Admin`)
* **Path Parameter**: `{orderId}`
* **Request Body JSON**:
```json
{
  "status": "Shipped",
  "title": "Đơn hàng xuất kho thành công",
  "description": "Đơn hàng đã giao cho bên vận chuyển Viettel Post.",
  "location": "Tổng kho GymKitten Q12, TP.HCM"
}
```

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "orderId": "b0000000-0000-0000-0000-000000000099",
    "status": "Shipped",
    "message": "Order status updated successfully to Shipped."
  },
  "error": null,
  "timestamp": "2026-08-31T12:55:00Z"
}
```

---

## 💻 4. VÍ DỤ INTEGRATION CODE FRONTEND (TYPESCRIPT / REACT)

```typescript
// 1. Types definition
export interface OrderTrackingHistoryDto {
  trackingId: string;
  orderId: string;
  status: string;
  title: string;
  description?: string;
  location?: string;
  timestamp: string;
  createdAt: string;
}

// 2. Fetch Tracking Timeline API
export async function getOrderTrackingApi(orderId: string): Promise<OrderTrackingHistoryDto[]> {
  const res = await fetch(`/api/orders/${orderId}/tracking`, {
    headers: {
      Authorization: `Bearer ${localStorage.getItem("token")}`,
    },
  });
  const json = await res.json();
  if (!json.isSuccess) throw new Error(json.error?.message || "Failed to fetch tracking history");
  return json.data;
}

// 3. Cancel Order API
export async function cancelMyOrderApi(orderId: string): Promise<void> {
  const res = await fetch(`/api/orders/${orderId}/cancel`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${localStorage.getItem("token")}`,
    },
  });
  const json = await res.json();
  if (!res.ok || !json.isSuccess) {
    throw new Error(json.detail || json.error?.message || "Cannot cancel order");
  }
}
```
