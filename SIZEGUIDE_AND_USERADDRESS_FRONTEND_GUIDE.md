# 📏 & 🏠 HƯỚNG DẪN TÍCH HỢP APIS BẢNG SIZE (SIZEGUIDE) & SỔ ĐỊA CHỈ (USERADDRESS)

> **Dành cho Frontend Developers**: Tài liệu hướng dẫn tích hợp Widget **Gợi ý Size Thông minh, Bảng Quy đổi Kích cỡ** và **Sổ Địa chỉ Nhận hàng 1-Click Checkout**.

---

## 📏 PART 1: FEATURE 4 — BẢNG QUY ĐỔI SIZE THÔNG MINH (SIZEGUIDE)

### 1.1 Lấy Bảng Quy Đổi Size Của Sản Phẩm (`GET /api/products/{productId}/size-guide`)

* **Response Model (`200 OK`)**:
```json
{
  "isSuccess": true,
  "data": {
    "productId": "p0000000-0000-0000-0000-000000000011",
    "items": [
      {
        "guideId": "g0000000-0000-0000-0000-000000000001",
        "size": "S",
        "chestCm": "84-88",
        "waistCm": "68-72",
        "hipsCm": "92-96",
        "heightRangeCm": "160-168"
      },
      {
        "guideId": "g0000000-0000-0000-0000-000000000002",
        "size": "M",
        "chestCm": "88-94",
        "waistCm": "72-78",
        "hipsCm": "96-102",
        "heightRangeCm": "168-175"
      }
    ]
  },
  "error": null,
  "timestamp": "2026-09-03T16:30:00Z"
}
```

---

### 1.2 Widget "Gợi Ý Size Cho Tôi" (`POST /api/size-guide/recommend`)

* **Request Body JSON**:
```json
{
  "productId": "p0000000-0000-0000-0000-000000000011",
  "heightCm": 172,
  "weightKg": 68,
  "chestCm": 91,
  "waistCm": 75
}
```

* **Response Model (`200 OK`)**:
```json
{
  "isSuccess": true,
  "data": {
    "recommendedSize": "M",
    "confidence": "95%",
    "explanation": "Số đo vòng ngực 91cm nằm trong khoảng 88-94cm của Size M."
  },
  "error": null,
  "timestamp": "2026-09-03T16:30:00Z"
}
```

---

### 1.3 Admin Thiết Lập Bảng Size (`POST /api/admin/products/{productId}/size-guide` & `PUT /api/admin/products/size-guide/{guideId}`)

* **Header**: `Authorization: Bearer <AdminAccessToken>`
* **Request Body JSON**:
```json
{
  "size": "L",
  "chestCm": "94-100",
  "waistCm": "78-84",
  "hipsCm": "102-108",
  "heightRangeCm": "175-182"
}
```

---

## 🏠 PART 2: FEATURE 5 — SỔ ĐỊA CHỈ NHẬN HÀNG (USERADDRESS)

---

### 2.1 Lấy Danh Sách Địa Chỉ Của Tôi (`GET /api/user/addresses`)

> ⚠️ **Yêu cầu**: Khách hàng đã đăng nhập (`[Authorize]`). Địa chỉ mặc định (`isDefault: true`) sẽ tự động nằm ở vị trí đầu tiên.

* **Header**: `Authorization: Bearer <AccessToken>`
* **Response Model (`200 OK`)**:
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "addressId": "a1000000-0000-0000-0000-000000000001",
        "receiverName": "Nguyen Van A",
        "phoneNumber": "0901234567",
        "addressLine1": "123 Le Loi",
        "ward": "Phường Ben Nghe",
        "district": "Quan 1",
        "city": "TP. Ho Chi Minh",
        "isDefault": true,
        "addressType": "Home",
        "createdAt": "2026-09-03T10:00:00Z"
      }
    ]
  },
  "error": null,
  "timestamp": "2026-09-03T16:30:00Z"
}
```

---

### 2.2 Thêm Địa Chỉ Mới (`POST /api/user/addresses`)

* **Header**: `Authorization: Bearer <AccessToken>`
* **Request Body JSON**:
```json
{
  "receiverName": "Nguyen Van A",
  "phoneNumber": "0901234567",
  "addressLine1": "456 Nguyen Hue",
  "ward": "Phong Ben Nghe",
  "district": "Quan 1",
  "city": "TP. Ho Chi Minh",
  "isDefault": true,
  "addressType": "Office"
}
```

---

### 2.3 Cập Nhật Địa Chỉ (`PUT /api/user/addresses/{addressId}`)

* **Header**: `Authorization: Bearer <AccessToken>`

---

### 2.4 Đặt Làm Địa Chỉ Mặc Định 1-Click (`PUT /api/user/addresses/{addressId}/set-default`)

* **Header**: `Authorization: Bearer <AccessToken>`

---

### 2.5 Xóa Địa Chỉ (`DELETE /api/user/addresses/{addressId}`)

* **Header**: `Authorization: Bearer <AccessToken>`
