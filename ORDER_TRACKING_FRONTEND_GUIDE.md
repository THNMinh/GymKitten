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
        "totalItems": 2,
        "items": [
          {
            "orderItemId": "c0000000-0000-0000-0000-000000000055",
            "variantId": "v0000000-0000-0000-0000-000000000011",
            "sku": "ONX-HOODIE-BLK-L",
            "productName": "Onyx Oversize Hoodie - Black / L",
            "unitPrice": 950000,
            "quantity": 2,
            "totalPrice": 1900000,
            "imageUrl": "http://localhost:9000/gymkitten-media/variants/hoodie-black-l.png"
          }
        ]
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "error": null,
  "timestamp": "2026-08-31T13:47:00Z"
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
        "totalPrice": 1900000,
        "imageUrl": "http://localhost:9000/gymkitten-media/variants/hoodie-black-l.png"
      }
    ]
  },
  "error": null,
  "timestamp": "2026-08-31T13:47:00Z"
}
```

---

### 2.3 Xem Lịch Sử Hành Trình Giao Hàng Timeline (`GET /api/orders/{orderId}/tracking`)

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
    }
  ],
  "error": null,
  "timestamp": "2026-08-31T13:47:00Z"
}
```

---

### 2.4 Khách Hàng Tự Bấm Hủy Đơn (`PUT /api/orders/{orderId}/cancel`)

* **Header**: `Authorization: Bearer <AccessToken>`
* **Path Parameter**: `{orderId}` - GUID của đơn hàng.

---

## 🛡️ 3. ADMIN APIS (QUẢN TRỊ VIÊN)

---

### 3.1 Quản Lý Danh Sách Tất Cả Đơn Hàng (`GET /api/admin/orders`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Bắt buộc Role `Admin`)

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
        "totalItems": 2,
        "items": [
          {
            "orderItemId": "c0000000-0000-0000-0000-000000000055",
            "variantId": "v0000000-0000-0000-0000-000000000011",
            "sku": "ONX-HOODIE-BLK-L",
            "productName": "Onyx Oversize Hoodie",
            "unitPrice": 950000,
            "quantity": 2,
            "totalPrice": 1900000,
            "imageUrl": "http://localhost:9000/gymkitten-media/variants/hoodie-black-l.png"
          }
        ]
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "error": null,
  "timestamp": "2026-08-31T13:47:00Z"
}
```
