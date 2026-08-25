# 📘 GymKitten API Documentation (Tài liệu Tích hợp Frontend)

Tài liệu này tổng hợp toàn bộ các **API Endpoint** hiện có trong backend ứng dụng **GymKitten**, bao gồm thông tin phương thức HTTP, đường dẫn (route), tham số (parameters), body request, dữ liệu trả về (response) và mục đích sử dụng.

Tài liệu được phân loại rõ ràng thành **Nhóm Admin (Quản trị hệ thống)** và **Nhóm Customer / Public (Khách hàng & Xác thực)** giúp đội ngũ Frontend dễ dàng xây dựng giao diện ứng dụng.

---

## 📋 Table of Contents (Mục lục)
1. [Thông tin chung & Chuẩn xử lý API](#1-thông-tin-chung--chuẩn-xử-lý-api)
2. [DANH SÁCH API QUẢN TRỊ (ADMIN PORTAL)](#2-danh-sách-api-quản-trị-admin-portal)
   - [2.1. Quản lý Đơn hàng Admin (`/api/admin/orders`)](#21-quản-lý-đơn-hàng-admin-apiadminorders)
   - [2.2. Quản lý Tồn kho Admin (`/api/admin/inventory`)](#22-quản-lý-tồn-kho-admin-apiadmininventory)
   - [2.3. Báo cáo Tài chính Admin (`/api/admin/finance`)](#23-báo-cáo-tài-chính-admin-apiadminfinance)
   - [2.4. Quản lý Danh mục Sản phẩm (`/api/categories`)](#24-quản-lý-danh-mục-sản-phẩm-apicategories)
   - [2.5. Quản lý Sản phẩm (`/api/products`)](#25-quản-lý-sản-phẩm-apiproducts)
   - [2.6. Quản lý Biến thể Sản phẩm (`/api/products/variants`)](#26-quản-lý-biến-thể-sản-phẩm-apiproductsvariants)
   - [2.7. Quản lý Hình ảnh Sản phẩm (`/api/products/images`)](#27-quản-lý-hình-ảnh-sản-phẩm-apiproductsimages)
3. [DANH SÁCH API KHÁCH HÀNG & CÔNG KHAI (CLIENT PORTAL)](#3-danh-sách-api-khách-hàng--công-khai-client-portal)
   - [3.1. Xác thực & Tài khoản (`/api/auth`)](#31-xác-thực--tài-khoản-apiauth)
   - [3.2. Đặt hàng & Thanh toán (`/api/checkout`)](#32-đặt-hàng--thanh-toán-apicheckout)
   - [3.3. Xử lý Cổng thanh toán & Webhook IPN (`/api/payment`)](#33-xử-lý-cổng-thanh-toán--webhook-ipn-apipayment)
   - [3.4. Danh sách Yêu thích (`/api/wishlists`)](#34-danh-sách-yêu-thích-apiwishlists)
4. [Hướng dẫn dành riêng cho Frontend Admin UI](#4-hướng-dẫn-dành-riêng-cho-frontend-admin-ui)

---

## 1. Thông tin chung & Chuẩn xử lý API

### 🔑 Authentication Header
Các API yêu cầu đăng nhập (`[Authorize]`) truyền Token qua HTTP Header:
```http
Authorization: Bearer <YOUR_ACCESS_TOKEN>
```

### ⚠️ Định dạng Error Response (ProblemDetails Standard)
Tất cả các API trả về lỗi đều tuân theo chuẩn `ProblemDetails` của RFC 7231 / RFC 7235:
```json
{
  "status": 400,
  "title": "Tên nhóm lỗi",
  "detail": "Mô tả chi tiết nguyên nhân lỗi",
  "extensions": {
    "code": "ERROR_CODE_NAME"
  }
}
```

---

## 2. DANH SÁCH API QUẢN TRỊ (ADMIN PORTAL)

### 2.1. Quản lý Đơn hàng Admin (`/api/admin/orders`)

#### ➔ `PUT /api/admin/orders/{orderId}/ship`
* **Tác dụng**: Chuyển trạng thái đơn hàng sang **"Shipped" (Đã giao cho đơn vị vận chuyển)**.
* **Xác thực**: Yêu cầu Token Quản trị (Admin).
* **URL Parameter**:
  * `orderId` (`Guid` - Required): ID duy nhất của đơn hàng.
* **Request Body**: Không có.
* **Response thành công (`200 OK`)**:
  ```json
  {
    "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Shipped",
    "shippedAt": "2026-08-20T18:45:00Z"
  }
  ```
* **Response thất bại (`400 Bad Request`)**: Đơn hàng không tồn tại, đã hủy hoặc không ở trạng thái hợp lệ để giao hàng.

---

### 2.2. Quản lý Tồn kho Admin (`/api/admin/inventory`)

#### ➔ `GET /api/admin/inventory`
* **Tác dụng**: Lấy danh sách tồn kho của các **Biến thể Sản phẩm (Product Variants)**, phục vụ màn hình bảng quản lý kho.
* **Xác thực**: Yêu cầu Admin Token.
* **Query Parameters**:
  * `sku` (`string`, Optional): Tìm kiếm theo mã SKU sản phẩm.
  * `productName` (`string`, Optional): Tìm kiếm theo tên sản phẩm.
  * `page` (`int`, Optional, Default: `1`): Trang hiện tại.
  * `pageSize` (`int`, Optional, Default: `20`): Số bản ghi trên mỗi trang.
* **Response thành công (`200 OK`)**:
  ```json
  {
    "items": [
      {
        "variantId": "d3b07384-d113-4b4e-9c90-038fc9e0d4a9",
        "sku": "GK-TEE-BLK-M",
        "productName": "GymKitten Essential Tee",
        "color": "Black",
        "size": "M",
        "quantityOnHand": 100,
        "quantityReserved": 5,
        "availableStock": 95
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
  ```

#### ➔ `POST /api/admin/inventory/restock`
* **Tác dụng**: Nhập hàng / Bổ sung thêm số lượng tồn kho cho 1 biến thể sản phẩm.
* **Xác thực**: Yêu cầu Admin Token.
* **Request Body (`application/json`)**:
  ```json
  {
    "variantId": "d3b07384-d113-4b4e-9c90-038fc9e0d4a9",
    "quantityAdded": 50,
    "note": "Nhập kho đợt hàng tháng 8/2026"
  }
  ```
* **Response thành công (`200 OK`)**: Trả về thông tin kho mới sau khi bổ sung.

#### ➔ `PUT /api/admin/inventory/adjust`
* **Tác dụng**: Điều chỉnh/Sửa trực tiếp số lượng kho thực tế (`QuantityOnHand`) (dùng cho kiểm kê, phát hiện chênh lệch/hỏng hóc).
* **Xác thực**: Yêu cầu Admin Token.
* **Request Body (`application/json`)**:
  ```json
  {
    "variantId": "d3b07384-d113-4b4e-9c90-038fc9e0d4a9",
    "newQuantityOnHand": 80,
    "reason": "Kiểm kê kho phát hiện thất thoát 5 sản phẩm"
  }
  ```
* **Response thành công (`200 OK`)**: Cập nhật thành công thông tin tồn kho.

---

### 2.3. Báo cáo Tài chính Admin (`/api/admin/finance`)

#### ➔ `GET /api/admin/finance/transactions`
* **Tác dụng**: Tra cứu danh sách các giao dịch thanh toán trong hệ thống (VnPay, MoMo, COD,...).
* **Xác thực**: Yêu cầu Admin Token.
* **Query Parameters**:
  * `startDate` (`DateTime`, Optional): Ngày bắt đầu (Format ISO 8601, ví dụ: `2026-08-01T00:00:00Z`).
  * `endDate` (`DateTime`, Optional): Ngày kết thúc.
  * `status` (`string`, Optional): Trạng thái giao dịch (`Success`, `Failed`, `Pending`).
  * `gateway` (`string`, Optional): Cổng thanh toán (`VnPay`, `MoMo`, `COD`).
  * `page` (`int`, Optional, Default: `1`).
  * `pageSize` (`int`, Optional, Default: `20`).
* **Response thành công (`200 OK`)**:
  ```json
  {
    "items": [
      {
        "transactionId": "b1a2c3d4-e5f6-7890-abcd-1234567890ab",
        "orderCode": "GK-ORD-20260820-001",
        "userEmail": "customer@gmail.com",
        "gateway": "VnPay",
        "amount": 450000.00,
        "status": "Success",
        "createdAt": "2026-08-20T10:00:00Z",
        "paymentDate": "2026-08-20T10:02:15Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
  ```

---

### 2.4. Quản lý Danh mục Sản phẩm (`/api/categories`)

#### ➔ `GET /api/categories`
* **Tác dụng**: Lấy danh sách danh mục (Hỗ trợ lọc & phân trang).
* **Query Parameters**: `searchName` (`string`), `parentCategoryId` (`Guid`), `page` (`int`), `pageSize` (`int`).

#### ➔ `GET /api/categories/{id}`
* **Tác dụng**: Xem chi tiết 1 danh mục theo ID (`Guid`).

#### ➔ `POST /api/categories` *(Admin)*
* **Tác dụng**: Tạo danh mục mới.
* **Request Body (`application/json`)**:
  ```json
  {
    "parentCategoryId": null,
    "name": "Áo Gym Nam",
    "slug": "ao-gym-nam",
    "description": "Bộ sưu tập áo tập gym nam cao cấp",
    "displayOrder": 1
  }
  ```
* **Response thành công (`201 Created`)**: Trả về `CategoryDto` vừa tạo.

#### ➔ `PUT /api/categories/{id}` *(Admin)*
* **Tác dụng**: Cập nhật thông tin danh mục.
* **URL Parameter**: `id` (`Guid`).
* **Request Body (`application/json`)**:
  ```json
  {
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "parentCategoryId": null,
    "name": "Áo Gym Nam - Mới",
    "slug": "ao-gym-nam",
    "description": "Mô tả mới",
    "displayOrder": 1
  }
  ```

#### ➔ `DELETE /api/categories/{id}` *(Admin)*
* **Tác dụng**: Xóa danh mục sản phẩm theo ID.
* **Response thành công (`204 No Content`)**.

---

### 2.5. Quản lý Sản phẩm (`/api/products`)

#### ➔ `GET /api/products`
* **Tác dụng**: Tìm kiếm & lấy danh sách sản phẩm với bộ lọc đa dạng (Front-end dùng cho cả trang Catalog lẫn Admin Product Table).
* **Query Parameters**:
  * `searchName` (`string`)
  * `gender` (`string`: `Men`, `Women`, `Unisex`)
  * `fitType` (`string`: `Slim`, `Regular`, `Oversized`,...)
  * `categoryId` (`Guid`)
  * `isActive` (`bool`: `true`/`false`)
  * `colors` (mảng `string`, ví dụ: `?colors=Black&colors=Red`)
  * `sizes` (mảng `string`, ví dụ: `?sizes=M&sizes=L`)
  * `minPrice` (`decimal`), `maxPrice` (`decimal`)
  * `page` (`int`), `pageSize` (`int`)

#### ➔ `GET /api/products/{id}`
* **Tác dụng**: Lấy chi tiết sản phẩm theo ID (`Guid`).

#### ➔ `GET /api/products/slug/{slug}`
* **Tác dụng**: Lấy chi tiết sản phẩm theo Slug (đường dẫn URL).

#### ➔ `POST /api/products` *(Admin)*
* **Tác dụng**: Tạo sản phẩm mới trong catalog.
* **Request Body (`application/json`)**:
  ```json
  {
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Áo Ba Lỗ Tập Gym Kitten Flex",
    "slug": "ao-ba-lo-gym-kitten-flex",
    "description": "Thấm hút mồ hôi, co giãn 4 chiều.",
    "fitType": "Slim",
    "gender": "Men"
  }
  ```
* **Response thành công (`201 Created`)**.

#### ➔ `PUT /api/products/{id}` *(Admin)*
* **Tác dụng**: Cập nhật thông tin sản phẩm và ẩn/hiện sản phẩm (`isActive`).
* **Request Body (`application/json`)**:
  ```json
  {
    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Áo Ba Lỗ Tập Gym Kitten Flex Pro",
    "slug": "ao-ba-lo-gym-kitten-flex-pro",
    "description": "Cập nhật chất liệu nâng cấp.",
    "fitType": "Slim",
    "gender": "Men",
    "isActive": true
  }
  ```

#### ➔ `DELETE /api/products/{id}` *(Admin)*
* **Tác dụng**: Xóa sản phẩm khỏi hệ thống.
* **Response thành công (`204 No Content`)**.

---

### 2.6. Quản lý Biến thể Sản phẩm (`/api/products/variants`)

Một sản phẩm có thể có nhiều biến thể (Màu sắc, Size, Giá tiền, SKU).

#### ➔ `GET /api/products/{productId}/variants`
* **Tác dụng**: Lấy danh sách biến thể của sản phẩm theo `productId`.

#### ➔ `POST /api/products/variants` *(Admin)*
* **Tác dụng**: Tạo biến thể mới cho sản phẩm.
* **Request Body (`application/json`)**:
  ```json
  {
    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sku": "GK-FLEX-BLK-L",
    "colorName": "Black",
    "colorHex": "#000000",
    "size": "L",
    "price": 299000.00,
    "originalPrice": 350000.00,
    "weightGrams": 200
  }
  ```
* **Response thành công (`201 Created`)**.

#### ➔ `PUT /api/products/variants/{variantId}` *(Admin)*
* **Tác dụng**: Cập nhật giá, màu sắc, size hoặc SKU của 1 biến thể.
* **Request Body (`application/json`)**:
  ```json
  {
    "variantId": "d3b07384-d113-4b4e-9c90-038fc9e0d4a9",
    "sku": "GK-FLEX-BLK-L",
    "colorName": "Black Premium",
    "colorHex": "#050505",
    "size": "L",
    "price": 279000.00,
    "originalPrice": 350000.00,
    "weightGrams": 200
  }
  ```

#### ➔ `DELETE /api/products/variants/{variantId}` *(Admin)*
* **Tác dụng**: Xóa 1 biến thể sản phẩm.
* **Response thành công (`204 No Content`)**.

---

### 2.7. Quản lý Hình ảnh Sản phẩm (`/api/products/images`)

#### ➔ `GET /api/products/{productId}/images`
* **Tác dụng**: Lấy danh sách toàn bộ ảnh thuộc sản phẩm `productId`.

#### ➔ `POST /api/products/{productId}/images` *(Admin)*
* **Tác dụng**: Upload mảng các file hình ảnh cho sản phẩm (Có thể gán riêng ảnh cho 1 biến thể cụ thể).
* **Content-Type**: `multipart/form-data`
* **Form Parameters**:
  * `photos` (`List<IFormFile>` - Required): Danh sách file ảnh binary upload.
  * `variantId` (`Guid`, Optional): ID biến thể áp dụng ảnh (nếu là ảnh riêng cho màu đó).
* **Response thành công (`201 Created`)**.

#### ➔ `DELETE /api/products/images/{imageId}` *(Admin)*
* **Tác dụng**: Xóa 1 bức ảnh sản phẩm theo `imageId`.
* **Response thành công (`204 No Content`)**.

---

## 3. DANH SÁCH API KHÁCH HÀNG & CÔNG KHAI (CLIENT PORTAL)

### 3.1. Xác thực & Tài khoản (`/api/auth`)

#### ➔ `POST /api/auth/register`
* **Tác dụng**: Đăng ký tài khoản khách hàng mới.
* **Request Body (`application/json`)**:
  ```json
  {
    "fullName": "Nguyen Van A",
    "email": "nguyenvana@gmail.com",
    "password": "Password123!"
  }
  ```
* **Response thành công (`201 Created`)**.
* **Response thất bại (`409 Conflict`)**: Email đã được đăng ký trước đó.

#### ➔ `POST /api/auth/login`
* **Tác dụng**: Đăng nhập lấy JWT Bearer Token.
* **Request Body (`application/json`)**:
  ```json
  {
    "email": "nguyenvana@gmail.com",
    "password": "Password123!"
  }
  ```
* **Response thành công (`200 OK`)**:
  ```json
  {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "d8e9f10a-1234-5678-9abc-def012345678"
  }
  ```
* **Response thất bại (`401 Unauthorized`)**: Sai tài khoản hoặc mật khẩu.

#### ➔ `POST /api/auth/refresh-token`
* **Tác dụng**: Cấp lại AccessToken khi token cũ hết hạn.
* **Request Body (`application/json`)**:
  ```json
  {
    "accessToken": "eyJhbGciOiJIUzI1...",
    "refreshToken": "d8e9f10a-1234-5678-9abc-def012345678"
  }
  ```
* **Response thành công (`200 OK`)**: Trả về `accessToken` và `refreshToken` mới.

---

### 3.2. Đặt hàng & Thanh toán (`/api/checkout`)

#### ➔ `POST /api/checkout`
* **Tác dụng**: Khách hàng tiến hành đặt hàng từ giỏ hàng.
* **Xác thực**: Yêu cầu Token (`[Authorize]`).
* **Request Body (`application/json`)**:
  ```json
  {
    "items": [
      {
        "variantId": "d3b07384-d113-4b4e-9c90-038fc9e0d4a9",
        "quantity": 2
      }
    ],
    "shippingAddress": "123 Đường Lê Văn Sỹ, Phường 13, Quận 3, TP.HCM",
    "paymentMethod": "VNPAY",
    "customerNote": "Giao hàng giờ hành chính"
  }
  ```
  *(Lưu ý: `paymentMethod` hỗ trợ các giá trị: `"COD"`, `"VNPAY"`, `"MOMO"`)*
* **Response thành công (`200 OK`)**:
  ```json
  {
    "orderId": "e2a1b0c9-4d5e-6f7a-8b9c-0d1e2f3a4b5c",
    "orderCode": "GK-ORD-20260820-099",
    "totalAmount": 598000.00,
    "status": "Pending",
    "paymentStatus": "Pending",
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?..." 
  }
  ```
  *(Nếu chọn thanh toán VNPay / MoMo, Frontend lấy trường `paymentUrl` để chuyển hướng khách hàng sang trang thanh toán)*

---

### 3.3. Xử lý Cổng thanh toán & Webhook IPN (`/api/payment`)

Cổng thanh toán tự động tương tác với các endpoint này:

* `GET /api/payment/vnpay-ipn` - Webhook nhận callback tự động từ Server VNPay.
* `GET /api/payment/vnpay-callback` - Trang chuyển hướng trình duyệt của VNPay sau khi khách bấm thanh toán (Tự động redirect về `{FrontendUrl}/orders/success?...`).
* `POST /api/payment/momo-ipn` - Webhook IPN nhận kết quả từ Server MoMo.
* `GET /api/payment/momo-return` - Chuyển hướng trình duyệt từ MoMo về giao diện Frontend (`{FrontendUrl}/orders/success?...`).

---

### 3.4. Danh sách Yêu thích (`/api/wishlists`)

*(Yêu cầu đăng nhập `[Authorize]`)*

#### ➔ `GET /api/wishlists`
* **Tác dụng**: Lấy danh sách sản phẩm yêu thích của người dùng hiện tại (Phân trang `page`, `pageSize`).

#### ➔ `POST /api/wishlists/toggle`
* **Tác dụng**: Thêm hoặc Bỏ một sản phẩm khỏi danh sách yêu thích.
* **Request Body (`application/json`)**:
  ```json
  {
    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
  ```

#### ➔ `DELETE /api/wishlists/products/{productId}`
* **Tác dụng**: Xóa trực tiếp sản phẩm khỏi danh sách yêu thích theo `productId`.

---

## 4. Hướng dẫn dành riêng cho Frontend Admin UI

Để xây dựng một trang **Admin Dashboard** hoàn chỉnh cho GymKitten, Frontend Dev có thể thiết kế các màn hình tương ứng với các nhóm API sau:

### 1️⃣ Màn hình Quản lý Kho hàng (Inventory Management)
* **API**: `GET /api/admin/inventory`, `POST /api/admin/inventory/restock`, `PUT /api/admin/inventory/adjust`.
* **Giao diện khuyên dùng**:
  * Bảng hiển thị: SKU, Tên sản phẩm, Màu, Size, Kho thực tế (`QuantityOnHand`), Kho đã giữ đơn (`QuantityReserved`), Tồn kho khả dụng (`AvailableStock`).
  * Nút **"Nhập kho (Restock)"**: Mở Modal truyền `variantId`, `quantityAdded`, `note`.
  * Nút **"Điều chỉnh kho (Adjust)"**: Mở Modal truyền `variantId`, `newQuantityOnHand`, `reason`.

### 2️⃣ Màn hình Quản lý Đơn hàng (Order Fulfillment)
* **API**: `PUT /api/admin/orders/{orderId}/ship`.
* **Giao diện khuyên dùng**:
  * Nút **"Xác nhận giao hàng"** bên cạnh mỗi đơn hàng đang ở trạng thái `Pending` hoặc `Processing`. Khi bấm sẽ gọi API `ship` để cập nhật trạng thái đơn.

### 3️⃣ Màn hình Báo cáo Tài chính & Giao dịch (Financial Transactions)
* **API**: `GET /api/admin/finance/transactions`.
* **Giao diện khuyên dùng**:
  * Bộ lọc Filter topbar: Khoảng ngày (`startDate`, `endDate`), Cổng thanh toán (`VnPay`, `MoMo`, `COD`), Trạng thái (`Success`, `Failed`).
  * Bảng lịch sử giao dịch: Mã đơn (`orderCode`), Khách hàng (`userEmail`), Cổng (`gateway`), Số tiền (`amount`), Trạng thái badge màu sắc, Thời gian giao dịch.

### 4️⃣ Màn hình Quản lý Catalog (Categories, Products, Variants & Images)
* **Categories**: Thêm/Sửa/Xóa cây danh mục sản phẩm.
* **Products**: Quản lý thông tin chung (Tên, Slug, Giới tính `Men/Women`, Form dáng `Slim/Regular`, Bật/Tắt hiển thị `isActive`).
* **Variants**: Thêm biến thể Màu + Size + SKU + Giá niêm yết + Giá khuyến mãi cho từng sản phẩm.
* **Images**: Upload ảnh theo từng biến thể màu hoặc ảnh chung của sản phẩm qua `multipart/form-data`.
