# 🎟️ HƯỚNG DẪN TÍCH HỢP APIS MÃ GIẢM GIÁ & VOUCHER KHUYẾN MÃI (COUPONS & COUPON USAGE)

> **Dành cho Frontend Developers**: Tài liệu hướng dẫn áp dụng **Mã Giảm Giá, Voucher Khuyến Mãi (Phần trăm / Tiền cố định)** cho luồng Giỏ hàng, Checkout và Quản lý Coupon Admin.

---

## 📌 1. LOẠI MÃ GIẢM GIÁ (DISCOUNT TYPES)

| Key (`DiscountType`) | Ý nghĩa | Công thức tính tiền giảm |
| :--- | :--- | :--- |
| `"Percentage"` | **Giảm theo phần trăm (%)** | `subtotal * (discountValue / 100)`. Nếu có `maxDiscountAmount` thì giảm tối đa bằng `maxDiscountAmount`. |
| `"FixedAmount"` | **Giảm tiền cố định (VND)** | Giảm trực tiếp `discountValue` (Ví dụ 50,000 VND). Số tiền giảm tối đa bằng `subtotal`. |

---

## 🛒 2. CLIENT APIS (GIAO DIỆN KHÁCH HÀNG)

---

### 2.1 Lấy Danh Sách Mã Giảm Giá Đang Hoạt Động (`GET /api/coupons/active`)

* **Mục đích**: Hiển thị danh sách các mã giảm giá hợp lệ ở trang Kho Voucher hoặc trang thanh toán.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "couponId": "c1000000-0000-0000-0000-000000000001",
        "code": "GYMKITTEN10",
        "discountType": "Percentage",
        "discountValue": 10.0,
        "minOrderValue": 300000.0,
        "maxDiscountAmount": 50000.0,
        "usageLimit": 100,
        "usedCount": 12,
        "startDate": "2026-09-01T00:00:00Z",
        "endDate": "2026-09-30T23:59:59Z"
      }
    ]
  },
  "error": null,
  "timestamp": "2026-09-02T23:30:00Z"
}
```

---

### 2.2 Thử Áp Dụng Mã Giảm Giá Đếm Tiền (`POST /api/coupons/apply`)

> ⚠️ **Yêu cầu**: Khách hàng đã đăng nhập (`[Authorize]`).

* **Header**: `Authorization: Bearer <AccessToken>`
* **Request Body JSON**:
```json
{
  "code": "GYMKITTEN10",
  "orderSubtotal": 500000
}
```

#### 🟢 Response khi thành công (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "code": "GYMKITTEN10",
    "discountType": "Percentage",
    "discountValue": 10.0,
    "discountAmount": 50000.0,
    "finalAmount": 450000.0,
    "message": "Coupon applied successfully."
  },
  "error": null,
  "timestamp": "2026-09-02T23:30:00Z"
}
```

#### 🔴 Các mã lỗi chi tiết (`400 Bad Request`):
- `Coupon.NotFound`: Mã giảm giá không tồn tại.
- `Coupon.Inactive`: Mã giảm giá đang bị tạm dừng.
- `Coupon.Expired`: Mã giảm giá đã hết hạn hoặc chưa đến ngày bắt đầu.
- `Coupon.UsageLimitReached`: Mã giảm giá đã hết lượt sử dụng.
- `Coupon.MinOrderValueNotMet`: Đơn hàng chưa đạt giá trị tối thiểu (`minOrderValue`).

---

### 2.3 Áp Dụng Mã Giảm Giá Khi Đặt Hàng Checkout (`POST /api/checkout`)

Khi đặt hàng thành công, truyền thêm trường `couponCode` vào body Checkout. Backend sẽ tự động trừ số tiền giảm, ghi lại đơn hàng và chèn 1 bản ghi vào bảng `Couponusage`:

* **Header**: `Authorization: Bearer <AccessToken>`
* **Request Body JSON**:
```json
{
  "items": [
    {
      "variantId": "v0000000-0000-0000-0000-000000000011",
      "quantity": 2
    }
  ],
  "shippingAddress": "123 Le Loi, Q1, TP.HCM",
  "paymentMethod": "MOMO",
  "customerNote": "Giao giờ hành chính",
  "couponCode": "GYMKITTEN10"
}
```

---

## 🛡️ 3. ADMIN APIS (QUẢN TRỊ VIÊN)

---

### 3.1 Xem Tất Cả Mã Giảm Giá Đã Phân Trang & Lọc (`GET /api/admin/coupons`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Role `Admin`)
* **Query Parameters**:
  - `code` *(Optional, string)*: Tìm kiếm theo mã giảm giá (ví dụ: `GYMKITTEN`).
  - `discountType` *(Optional, string)*: Lọc theo loại (`Percentage` / `FixedAmount`).
  - `isActive` *(Optional, boolean)*: Lọc theo trạng thái active (`true` / `false`).
  - `page` *(Default: 1)*: Trang hiện tại.
  - `pageSize` *(Default: 10)*: Số lượng mã trên 1 trang.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "couponId": "c1000000-0000-0000-0000-000000000001",
        "code": "GYMKITTEN10",
        "discountType": "Percentage",
        "discountValue": 10.0,
        "minOrderValue": 300000.0,
        "maxDiscountAmount": 50000.0,
        "usageLimit": 100,
        "usedCount": 12,
        "startDate": "2026-09-01T00:00:00Z",
        "endDate": "2026-09-30T23:59:59Z",
        "isActive": true,
        "createdAt": "2026-09-01T00:00:00Z",
        "updatedAt": "2026-09-01T00:00:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "error": null,
  "timestamp": "2026-09-03T00:36:00Z"
}
```

---

### 3.2 Tạo Mã Giảm Giá Mới (`POST /api/admin/coupons`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Role `Admin`)
* **Request Body JSON**:
```json
{
  "code": "GYMKITTEN10",
  "discountType": "Percentage",
  "discountValue": 10.0,
  "minOrderValue": 300000.0,
  "maxDiscountAmount": 50000.0,
  "usageLimit": 100,
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-30T23:59:59Z",
  "isActive": true
}
```

---

### 3.3 Cập Nhật Mã Giảm Giá (`PUT /api/admin/coupons/{id}`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Role `Admin`)

---

### 3.4 Xóa Mềm Mã Giảm Giá (`DELETE /api/admin/coupons/{id}`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Role `Admin`)
* **Cơ chế**: Đánh dấu `DeletedAt = DateTime.UtcNow` và `IsActive = false`. Mã sẽ không còn hiển thị ở giao diện active nhưng lịch sử đơn hàng cũ dùng mã này vẫn giữ nguyên trong DB.
