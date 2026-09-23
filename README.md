# 🐱 GymKitten – High-Performance E-Commerce Web API (.NET 8)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4)](https://docs.microsoft.com/en-us/ef/core/)
[![MediatR](https://img.shields.io/badge/MediatR-CQRS-orange)](https://github.com/jbogard/MediatR)
[![Redis](https://img.shields.io/badge/Redis-Distributed_Cache-DC382D?logo=redis)](https://redis.io/)
[![Hangfire](https://img.shields.io/badge/Hangfire-Background_Jobs-red)](https://www.hangfire.io/)
[![SignalR](https://img.shields.io/badge/SignalR-WebSockets-blue)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![MiniO](https://img.shields.io/badge/MiniO-pink)](https://www.min.io)

**GymKitten Backend** là hệ thống RESTful Web API thương mại điện tử chuyên ngành thời trang thể thao cao cấp (Gymshark-style). Hệ thống được thiết kế và xây dựng theo chuẩn mực **Clean Architecture** kết hợp mô hình **CQRS (Command Query Responsibility Segregation)**, tối ưu cho bài toán biến thể sản phẩm phức tạp, kiểm soát tồn kho đồng thời và thanh toán trực tuyến bảo mật.

🔗 **Swagger / API Documentation:** [https://gymkitten-api.onrender.com/swagger](https://gymkitten-api.onrender.com/swagger)  
🔗 **Frontend Web Application:** [https://gym-style-hub.vercel.app/](https://gym-style-hub.vercel.app/)

---

## 🏛 Kiến trúc Hệ thống (Clean Architecture)

Dự án tuân thủ nghiêm ngặt nguyên lý **Dependency Inversion** và phân tách thành 4 tầng rõ ràng:

```
                            ┌────────────────────────┐
                            │     GymKitten.API      │ (Presentation / Controllers)
                            └───────────┬────────────┘
                                        │
                                        ▼
                            ┌────────────────────────┐
                            │ GymKitten.Application  │ (Use Cases / CQRS MediatR)
                            └───────────┬────────────┘
                                        │
                    ┌───────────────────┴───────────────────┐
                    ▼                                       ▼
        ┌────────────────────────┐              ┌────────────────────────┐
        │   GymKitten.Domain     │              │ GymKitten.Infrastructure│
        │ (Entities & Rules)     │              │ (EF Core, Redis, Jobs) │
        └────────────────────────┘              └────────────────────────┘
```

### 1. `GymKitten.Domain` (Core Business Rules)
- Là hạt nhân độc lập, không tham chiếu bất kỳ thư viện bên ngoài hay cơ sở dữ liệu nào.
- Chứa các Entities (*Product, ProductVariant, SizeGuide, Order, InventoryItem, User, Promotion...*), Enums, Value Objects, Domain Events và Domain Errors.

### 2. `GymKitten.Application` (Use Cases & Business Logic)
- Triển khai **CQRS** thông qua **MediatR**: Phân tách rành mạch Commands (tác vụ ghi/cập nhật) và Queries (truy vấn dữ liệu tối ưu).
- **Pipeline Behaviors:** Tự động hóa Validation dữ liệu (FluentValidation), Logging request và quản lý Transaction nhất quán.
- Khai báo các abstractions (*IRepository, IUnitOfWork, ICurrentUserService, IEmailService, IPaymentService, IStorageService*).

### 3. `GymKitten.Infrastructure` (Data Access & External Services)
- **Database:** Entity Framework Core 8 trên **PostgreSQL**, chia tách schema nghiệp vụ chuyên biệt (`catalog`, `identity`, `order`, `inventory`, `promotion`, `social_proof`, `payment`, `system`).
- **Distributed Caching:** Tích hợp **Redis** làm hàng đợi tam thời xử lý ngầm khi gửi mã OTP, mật khẩu.
- **Background Jobs:** Tích hợp **Hangfire** phục vụ xử lý gửi mail OTP, quét đơn hàng quá hạn thanh toán.
- **Payment Gateways:** Tích hợp MoMo & VNPay với chữ ký số bảo mật HMAC SHA256.
- **Media Storage:** Hỗ trợ Cloudinary & MinIO S3-compatible storage.
- **Real-time:** Quản trị kết nối **SignalR WebSockets Hub**.

### 4. `GymKitten.API` (Presentation Layer)
- Cung cấp RESTful Web API với Swagger / OpenAPI UI tương tác trực quan.
- **Global Exception Middleware:** Xử lý ngoại lệ tập trung, trả về response định dạng chuẩn **RFC 7807 (ProblemDetails)**.
- **Security:** Xác thực & phân quyền JWT Bearer (Role-based: Admin vs Customer).

---

## ⚡ Các Tính năng Kỹ thuật Cốt lõi (Key Features)

### 1. Engine Biến thể Sản phẩm (Product Variants & Catalog Engine)
- Thiết kế mô hình dữ liệu quan hệ cho sản phẩm thời trang đa chiều: Mỗi sản phẩm chứa nhiều biến thể kết hợp giữa **Màu sắc** (tên màu, mã Hex, bộ ảnh riêng biệt) và **Kích cỡ** (XS, S, M, L, XL), đi kèm SKU chuẩn và bảng Size Guide đo đạc theo cm.
- Hỗ trợ tìm kiếm toàn văn, lọc đa thuộc tính (Gender, Category, Fit type, Khoảng giá, Tỷ lệ giảm giá).

### 2. Quản lý Tồn kho & Kiểm soát Tranh chấp (Inventory & Concurrency Control)
- Quản lý số lượng tồn kho (*QuantityOnHand*) và số lượng giữ trước (*QuantityReserved*) theo từng SKU.
- Ứng dụng **Database Transactions** và kiểm soát concurrency để loại bỏ triệt để nguy cơ **Overselling** khi nhiều người cùng checkout một sản phẩm tại cùng một thời điểm.

### 3. Quy trình Đặt hàng & Cổng Thanh toán (Checkout & Payments)
- Vòng đời đơn hàng khép kín: Giỏ hàng -> Áp dụng Voucher hợp lệ -> Đặt hàng -> Cổng thanh toán (MoMo / VNPay).
- Xử lý Webhook / IPN bảo mật với chữ ký số **HMAC SHA256**, đảm bảo tính toàn vẹn và idempotency khi nhận callback từ bên thứ 3.

### 4. Xác thực & Bảo mật (Identity & Security)
- Stateless Authentication với **JWT Access Token & Refresh Token rotation**.
- Mã hóa mật khẩu an toàn với thuật toán **BCrypt**.
- Cơ chế kích hoạt tài khoản bằng mã **OTP 6 chữ số** gửi qua email (MailKit) với thời gian hết hạn nghiêm ngặt (5 phút).

### 5. Xử lý Chạy nền & Thời gian thực (Background Processing & Real-time)
- **Hangfire Dashboard:** Giám sát trực quan các background job định kỳ (quét đơn treo, dọn dẹp token).
- **SignalR Real-time:** Đẩy thông báo trạng thái đơn hàng tới khách hàng và đồng bộ hóa đánh giá sản phẩm tức thì không cần tải lại trang.

---

## 📂 Cấu trúc Cơ sở dữ liệu (Database Schemas)

PostgreSQL được phân tách thành 8 schema rõ ràng:
- `identity`: Users, Roles, UserAddresses, RefreshTokens.
- `catalog`: Categories, Products, ProductVariants, ProductImages, SizeGuides.
- `inventory`: InventoryItems, StockTransactions.
- `order`: Carts, CartItems, Orders, OrderItems, OrderHistories.
- `promotion`: Coupons, CouponUsages.
- `social_proof`: ProductReviews, ReviewImages, Wishlists.
- `payment`: PaymentTransactions, PaymentLogs.
- `system`: SystemLogs, Notifications.

---

## 🚀 Khởi chạy dự án Local

### Yêu cầu môi trường:
- **.NET 8 SDK**
- **PostgreSQL 15+**
- **Redis Server** (hoặc dùng Docker)

### Các bước thực hiện:

1. **Clone repository:**
   ```bash
   git clone https://github.com/THNMinh/GymKitten.git
   cd GymKitten/GymKitten_Backend
   ```

2. **Cấu hình chuỗi kết nối (`appsettings.json`):**
   Sao chép file `appsettings.Example.json` thành `appsettings.json` và cập nhật thông tin:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=gymkitten;Username=postgres;Password=your_password"
     },
     "JwtSettings": {
       "Secret": "YourSuperSecretKeyForGymKittenProject2026!",
       "Issuer": "GymKittenAPI",
       "Audience": "GymKittenClient"
     }
   }
   ```

3. **Chạy Migration & Seed Database:**
   ```bash
   dotnet ef database update --project GymKitten.Infrastructure --startup-project GymKitten.API
   ```
   *(Tùy chọn: Chạy các script seed dữ liệu mẫu sản phẩm Gymshark có sẵn trong thư mục gốc như `seed_cbum_and_collab_collection.sql`, `seed_power_collection.sql`...)*

4. **Khởi chạy ứng dụng:**
   ```bash
   dotnet run --project GymKitten.API
   ```
   Mở trình duyệt truy cập Swagger UI: `http://localhost:5000/swagger`

---

## ☁️ Triển khai & Cloud Hosting (Deployment)

Hệ thống được đóng gói container hóa và triển khai tự động theo mô hình Cloud PaaS:

- **Docker Multi-Stage Build:** Tối ưu hóa kích thước image bằng cách chia tách build stage (.NET 8 SDK) và runtime stage (ASP.NET Core 8 Alpine/Debian runtime nhẹ).
- **Backend API Hosting:** Triển khai trên **Render Web Service** với HTTPS tự động, kết nối trực tiếp với **Managed PostgreSQL Cloud Database**.
- **Continuous Deployment (CI/CD):** Tự động trigger build và deploy khi có commit mới vào nhánh `main` trên GitHub.
- **Frontend Hosting:** Triển khai độc lập trên **Vercel Edge Network** ([gym-style-hub.vercel.app](https://gym-style-hub.vercel.app/)), tối ưu CDN toàn cầu.

