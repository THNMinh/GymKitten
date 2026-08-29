# 📘 API REFACTORING & FRONTEND MIGRATION GUIDE

> **Dành cho Frontend Developers**: Tài liệu tổng hợp toàn bộ các thay đổi về cấu trúc API Backend (Phiên bản Legacy vs Phiên bản Refactored) để Frontend cập nhật tích hợp nhanh chóng, chính xác.

---

## 📌 1. TỔNG QUAN CÁC THAY ĐỔI CHÍNH (KEY ARCHITECTURAL CHANGES)

| Hạng mục | Phiên bản Cũ (Legacy) | Phiên bản Mới (Refactored) | Ảnh hưởng Frontend |
| :--- | :--- | :--- | :--- |
| **Response Format** | Trả về trực tiếp Object raw hoặc `ProblemDetails` không đồng nhất. | Chuẩn hóa Envelope `ApiResult<T>` cho tất cả kết quả trả về. | Cần bóc tách `response.data` thay vì đọc thẳng `response`. |
| **Cập nhật dữ liệu (`PUT`)** | Phải truyền `ID` **2 lần** (Vừa trên URL path vừa trong JSON Body). | **Loại bỏ hoàn toàn ID trong JSON Body** đối với các API `PUT`. | Bỏ trường `id` / `categoryId` / `productId` / `variantId` khỏi Body request `PUT`. |
| **Authentication Payloads** | `RegisterCustomerCommand` nhận `Email`, `Password`, v.v. | `RegisterRequest` nhận `{ fullName, email, password }`. `RefreshTokenRequest` nhận `{ accessToken, refreshToken }`. | Cập nhật lại key payload khi gọi Auth API. |
| **Dual Route Alias** | Đường dẫn Payment chỉ có 1 alias `/api/payment`. | Hỗ trợ cả 2 alias `/api/payment` và `/api/payments`. | Đảm bảo MoMo/VNPay IPN Webhook hoạt động mượt mà. |

---

## 🧱 2. CHUẨN HÓA CẤU TRÚC RESPONSE ENVELOPE (`ApiResult<T>`)

Tất cả các API thành công hiện tại đều trả về cấu trúc **Response Envelope** đồng nhất:

```json
{
  "isSuccess": true,
  "data": {
    /* Dữ liệu trả về của API */
  },
  "error": null,
  "timestamp": "2026-08-29T10:15:00Z"
}
```

### 💡 Ví dụ hàm Helper request ở Frontend (TypeScript):

```typescript
// Interface của Response Envelope từ Backend
export interface ApiResult<T> {
  isSuccess: boolean;
  data: T;
  error: { code: string; message: string } | null;
  timestamp: string;
}

// Ví dụ hàm fetch wrapper
export async function fetchApi<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, options);
  const json: ApiResult<T> = await res.json();
  
  if (!json.isSuccess) {
    throw new Error(json.error?.message || "API Request Failed");
  }

  return json.data; // Trả về trực tiếp data bên trong envelope
}
```

---

## 🔄 3. SO SÁNH CHI TIẾT THEO TỪNG CONTROLLER

---

### 1. Categories API (`/api/categories`)

#### 🔴 Trước Refactor (Legacy)
- `PUT /api/categories/{id}`: Phải gửi `id` trong body:
  ```json
  {
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Gym Apparel",
    "slug": "gym-apparel",
    "description": "Gym Clothes",
    "displayOrder": 1
  }
  ```

#### 🟢 Sau Refactor (Mới)
- `PUT /api/categories/{id}`: **KHÔNG còn gửi `categoryId` trong body**:
  ```json
  {
    "parentCategoryId": null,
    "name": "Gym Apparel",
    "slug": "gym-apparel",
    "description": "Gym Clothes",
    "displayOrder": 1
  }
  ```

---

### 2. Products API (`/api/products`)

#### 🔴 Trước Refactor (Legacy)
- `PUT /api/products/{id}`: Phải gửi `productId` trong body.

#### 🟢 Sau Refactor (Mới)
- `PUT /api/products/{id}`: **KHÔNG còn gửi `productId` trong body**:
  ```json
  {
    "categoryId": "c0000000-0000-0000-0000-000000000001",
    "name": "Onyx V1 Hoodie",
    "slug": "onyx-v1-hoodie",
    "description": "Premium oversize hoodie",
    "fitType": "Oversize",
    "gender": "Men",
    "isActive": true
  }
  ```

---

### 3. Product Variants API (`/api/products/variants`)

#### 🔴 Trước Refactor (Legacy)
- `PUT /api/products/variants/{variantId}`: Phải gửi `variantId` trong body.

#### 🟢 Sau Refactor (Mới)
- `PUT /api/products/variants/{variantId}`: **KHÔNG còn gửi `variantId` trong body**:
  ```json
  {
    "sku": "ONX-V1-LGRY-L",
    "colorName": "Light Grey",
    "colorHex": "#F7FF00",
    "size": "L",
    "price": 950000,
    "originalPrice": 1100000,
    "weightGrams": 500
  }
  ```

---

### 4. Auth API (`/api/auth`)

#### 🔴 Trước Refactor (Legacy)
- `POST /api/auth/register`:
  Chấp nhận `Email`, `Password`, `FullName` không nhất quán.
- `POST /api/auth/refresh-token`:
  Chỉ nhận `RefreshToken` đơn lẻ.

#### 🟢 Sau Refactor (Mới)
- `POST /api/auth/register`:
  ```json
  {
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "password": "Password123!"
  }
  ```
- `POST /api/auth/refresh-token`:
  ```json
  {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "7d9a1f2e-..."
  }
  ```

---

### 5. Checkout API (`/api/checkout`)

#### 🟢 Mẫu Request Chuẩn:
- `POST /api/checkout` (Yêu cầu Header `Authorization: Bearer <Token>`):
  ```json
  {
    "items": [
      {
        "variantId": "a0000000-0000-0000-0000-000000000011",
        "quantity": 2
      }
    ],
    "shippingAddress": "123 Le Loi, Quan 1, TP.HCM",
    "paymentMethod": "VNPAY", // "VNPAY" | "MOMO" | "COD"
    "customerNote": "Giao trong gio hanh chinh"
  }
  ```

#### 🟢 Mẫu Response Chuẩn:
```json
{
  "isSuccess": true,
  "data": {
    "orderId": "b0000000-0000-0000-0000-000000000099",
    "orderCode": "GK-2608291015-123",
    "totalAmount": 1900000,
    "currentStatus": "Pending",
    "paymentStatus": "Unpaid",
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?..."
  },
  "error": null,
  "timestamp": "2026-08-29T10:15:00Z"
}
```

---

### 6. Admin Inventory API (`/api/admin/inventory`)

#### 🟢 Restock (`POST /api/admin/inventory/restock`):
```json
{
  "variantId": "a0000000-0000-0000-0000-000000000011",
  "quantity": 50,
  "referenceId": "RESTOCK-BATCH-001"
}
```

#### 🟢 Adjust Stock (`PUT /api/admin/inventory/adjust`):
```json
{
  "variantId": "a0000000-0000-0000-0000-000000000011",
  "newQuantity": 100,
  "note": "Kiem ke kho dinh ky"
}
```

---

### 7. Wishlists API (`/api/wishlists`)

#### 🟢 Toggle Wishlist (`POST /api/wishlists/toggle`):
```json
{
  "productId": "b0000000-0000-0000-0000-000000000011"
}
```

#### 🟢 Delete Wishlist (`DELETE /api/wishlists/products/{productId}`):
- URL Path Parameter: `{productId}`

---

## 📊 4. TỔNG HỢP DANH SÁCH BẢNG API ENDPOINTS (FULL CHEATSHEET)

| Controller | Method | Endpoint Path | Description | Payload Body (Nếu có) |
| :--- | :--- | :--- | :--- | :--- |
| **Auth** | `POST` | `/api/auth/register` | Đăng ký tài khoản | `{ fullName, email, password }` |
| | `POST` | `/api/auth/login` | Đăng nhập lấy Token | `{ email, password }` |
| | `POST` | `/api/auth/refresh-token` | Đổi Access Token mới | `{ accessToken, refreshToken }` |
| **Categories** | `GET` | `/api/categories` | Lấy danh sách danh mục | Query params: `page`, `pageSize`, `search` |
| | `GET` | `/api/categories/{id}` | Lấy chi tiết danh mục | Path param `id` |
| | `POST` | `/api/categories` | Tạo danh mục mới | `{ parentCategoryId, name, slug, description, displayOrder }` |
| | `PUT` | `/api/categories/{id}` | Cập nhật danh mục | `{ parentCategoryId, name, slug, description, displayOrder }` *(Không cần ID)* |
| | `DELETE`| `/api/categories/{id}` | Xóa danh mục | Path param `id` |
| **Products** | `GET` | `/api/products` | Lấy danh sách sản phẩm | Query params: `page`, `pageSize`, `categoryId`, `gender` |
| | `GET` | `/api/products/{id}` | Lấy sản phẩm theo ID | Path param `id` |
| | `GET` | `/api/products/slug/{slug}` | Lấy sản phẩm theo Slug | Path param `slug` |
| | `POST` | `/api/products` | Tạo sản phẩm mới | `{ categoryId, name, slug, description, fitType, gender }` |
| | `PUT` | `/api/products/{id}` | Cập nhật sản phẩm | `{ categoryId, name, slug, description, fitType, gender, isActive }` *(Không cần ID)* |
| | `DELETE`| `/api/products/{id}` | Xóa sản phẩm | Path param `id` |
| **Variants** | `GET` | `/api/products/{productId}/variants` | Lấy biến thể của SP | Path param `productId` |
| | `POST` | `/api/products/variants` | Tạo biến thể sản phẩm | `{ productId, sku, colorName, colorHex, size, price, originalPrice, weightGrams }` |
| | `PUT` | `/api/products/variants/{variantId}` | Cập nhật biến thể | `{ sku, colorName, colorHex, size, price, originalPrice, weightGrams }` *(Không cần ID)* |
| | `DELETE`| `/api/products/variants/{variantId}` | Xóa biến thể | Path param `variantId` |
| **Images** | `GET` | `/api/products/{productId}/images` | Lấy danh sách hình ảnh | Path param `productId` |
| | `POST` | `/api/products/{productId}/images` | Upload ảnh sản phẩm | Form Data: `photos`, `variantId` |
| | `DELETE`| `/api/products/images/{imageId}` | Xóa ảnh | Path param `imageId` |
| **Checkout** | `POST` | `/api/checkout` | Tạo đơn hàng & lấy link TT | `{ items: [{ variantId, quantity }], shippingAddress, paymentMethod, customerNote }` |
| **Wishlist** | `GET` | `/api/wishlists` | Lấy danh sách yêu thích | Header Token |
| | `POST` | `/api/wishlists/toggle` | Thêm/Xóa khỏi yêu thích | `{ productId }` |
| | `DELETE`| `/api/wishlists/products/{productId}` | Xóa khỏi yêu thích | Path param `productId` |
| **Admin** | `GET` | `/api/admin/inventory` | Lấy tồn kho thực tế | Query params: `page`, `pageSize` |
| | `POST` | `/api/admin/inventory/restock` | Nhập kho | `{ variantId, quantity, referenceId }` |
| | `PUT` | `/api/admin/inventory/adjust` | Điều chỉnh tồn kho | `{ variantId, newQuantity, note }` |
| | `PUT` | `/api/admin/orders/{orderId}/ship` | Đổi đơn sang Shipped | Path param `orderId` |
| | `GET` | `/api/admin/finance/transactions` | Lịch sử giao dịch TC | Query params: `startDate`, `endDate`, `gateway`, `status` |

---
*Tài liệu này được cập nhật tự động theo phiên bản Backend mới nhất.*
