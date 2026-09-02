# ⭐ HƯỚNG DẪN TÍCH HỢP APIS ĐÁNH GIÁ SẢN PHẨM & PHẢN HỒI FORM DÁNG (PRODUCT REVIEWS & FIT FEEDBACK)

> **Dành cho Frontend Developers**: Tài liệu hướng dẫn thiết kế giao diện **Đánh giá Sản phẩm, Phản hồi Form dáng (TrueToSize / RunsSmall / RunsLarge) & Upload Ảnh/Video trải nghiệm mặc thử** cho cả Client và Admin.

---

## 📌 1. TỔNG QUAN HỆ THỐNG FIT FEEDBACK (ENUMS)

Mục phản hồi form dáng phục vụ khách hàng chọn đúng size đồ tập:

| Value Key | Nhãn hiển thị gợi ý | Ý nghĩa |
| :--- | :--- | :--- |
| `"TrueToSize"` | **Vừa vặn chuẩn size** | Đồ mặc lên đúng như bảng size chuẩn. |
| `"RunsSmall"` | **Form hơi nhỏ (Nên tăng 1 size)** | Đồ bị bó / chật so với bảng size. |
| `"RunsLarge"` | **Form rộng rãi (Nên giảm 1 size)** | Đồ 
---

## 🛒 2. CLIENT APIS (GIAO DIỆN KHÁCH HÀNG)

---
bị rộng / thoải mái hơn chuẩn. |

### 2.1 Lấy Danh Sách Đánh Giá Của 1 Sản Phẩm (`GET /api/products/{productId}/reviews`)

* **Query Parameters**:
  - `rating` *(Optional, 1-5)*: Lọc theo số sao.
  - `hasMedia` *(Optional, true/false)*: Lọc bài đánh giá có ảnh/video mặc thử.
  - `page` *(Default: 1)*: Số trang.
  - `pageSize` *(Default: 10)*: Số lượng bài đánh giá trên 1 trang.

#### 🟢 Response Model (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "averageRating": 4.8,
    "totalReviews": 25,
    "ratingBreakdown": {
      "fiveStar": 20,
      "fourStar": 4,
      "threeStar": 1,
      "twoStar": 0,
      "oneStar": 0
    },
    "fitFeedbackSummary": {
      "trueToSizeCount": 18,
      "runsSmallCount": 5,
      "runsLargeCount": 2,
      "trueToSizePercentage": 72.0,
      "runsSmallPercentage": 20.0,
      "runsLargePercentage": 8.0
    },
    "items": [
      {
        "reviewId": "r0000000-0000-0000-0000-000000000001",
        "productId": "p0000000-0000-0000-0000-000000000011",
        "userId": "u0000000-0000-0000-0000-000000000022",
        "userFullName": "Nguyen Van A",
        "userAvatarUrl": "http://localhost:9000/gymkitten-media/avatars/userA.png",
        "rating": 5,
        "comment": "Áo mặc co giãn cực tốt, tập squat thoải mái!",
        "fitFeedback": "TrueToSize",
        "createdAt": "2026-09-01T10:00:00Z",
        "mediaList": [
          {
            "mediaId": "m0000000-0000-0000-0000-000000000001",
            "mediaUrl": "http://localhost:9000/gymkitten-media/reviews/tryon.mp4",
            "mediaType": "video"
          }
        ]
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3
  },
  "error": null,
  "timestamp": "2026-09-01T11:42:00Z"
}
```

---

### 2.2 Tạo Đánh Giá Sản Phẩm Trải Nghiệm Mặc Thử (`POST /api/reviews`)

> ⚠️ **Điều kiện**: Khách hàng cần đăng nhập (`[Authorize]`) và đã mua sản phẩm này trong đơn hàng (`orderId`).

* **Header**: `Authorization: Bearer <AccessToken>`
* **Request Body JSON**:
```json
{
  "productId": "p0000000-0000-0000-0000-000000000011",
  "orderId": "b0000000-0000-0000-0000-000000000099",
  "rating": 5,
  "comment": "Chất vải thun lạnh rất mát, form ôm tôn dáng chuẩn gym!",
  "fitFeedback": "TrueToSize"
}
```

#### 🟢 Response khi thành công (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "reviewId": "r0000000-0000-0000-0000-000000000001"
  },
  "error": null,
  "timestamp": "2026-09-01T11:42:00Z"
}
```

#### 🔴 Các mã lỗi nghiệp vụ chi tiết (`400 Bad Request`):
- `Review.InvalidRating`: Số sao phải từ 1 đến 5.
- `Review.InvalidFitFeedback`: FitFeedback phải thuộc một trong 3 giá trị: `TrueToSize`, `RunsSmall`, `RunsLarge`.
- `Review.ProductNotInOrder`: Đơn hàng này không chứa sản phẩm mà bạn đang đánh giá.
- `Review.AlreadyReviewed`: Bạn đã đánh giá sản phẩm này cho đơn hàng này rồi.

---

### 2.3 Upload Ảnh / Video Mặc Thử Cho Bài Đánh Giá (`POST /api/reviews/{reviewId}/media`)

* **Header**: `Authorization: Bearer <AccessToken>`
* **Content-Type**: `multipart/form-data`
* **Form Field Name**: `files` (Mảng tập tin ảnh/video)

#### 🟢 Response khi thành công (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "reviewId": "r0000000-0000-0000-0000-000000000001",
    "mediaList": [
      {
        "mediaId": "m0000000-0000-0000-0000-000000000001",
        "mediaUrl": "http://localhost:9000/gymkitten-media/reviews/photo1.png",
        "mediaType": "image"
      },
      {
        "mediaId": "m0000000-0000-0000-0000-000000000002",
        "mediaUrl": "http://localhost:9000/gymkitten-media/reviews/video1.mp4",
        "mediaType": "video"
      }
    ]
  },
  "error": null,
  "timestamp": "2026-09-01T11:42:00Z"
}
```

---

## 🛡️ 3. ADMIN APIS (QUẢN TRỊ VIÊN KIỂM DUYỆT)

---

### 3.1 Admin Xem Tất Cả Đánh Giá Trên Toàn Hệ Thống (`GET /api/admin/reviews`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Bắt buộc Role `Admin`)
* **Query Parameters**: `productId`, `rating`, `page`, `pageSize`.

---

### 3.2 Admin Xóa Đánh Giá Spam / Vi Phạm (`DELETE /api/admin/reviews/{reviewId}`)

* **Header**: `Authorization: Bearer <AdminAccessToken>` (Bắt buộc Role `Admin`)

#### 🟢 Response khi thành công (`200 OK`):
```json
{
  "isSuccess": true,
  "data": true,
  "error": null,
  "timestamp": "2026-09-01T11:42:00Z"
}
```
