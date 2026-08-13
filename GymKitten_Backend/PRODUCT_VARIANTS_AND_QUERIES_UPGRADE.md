# Document Update: Embedding Variants in Product Detail APIs (`GET /api/products/{id}` & `GET /api/products/slug/{slug}`)

## 📌 Executive Summary
Integrated `List<ProductVariantDto> Variants` into the `ProductDetailDto` returned by `GET /api/products/{id}` and `GET /api/products/slug/{slug}`. Frontend can now build single-product detail pages (Colors, Sizes, Pricing, Stock Availability) in a single request with 0 extra client round-trips.

---

## 🛠️ Changes Implemented in Backend (`GymKitten_Backend`)

### 1. **Updated DTOs (`ProductDetailDto` & `ProductVariantDto`)**
- Added `int Available` to `ProductVariantDto` (calculates available inventory stock: `QuantityOnHand - QuantityReserved`, fallback `10`).
- Added `List<ProductVariantDto> Variants` to `ProductDetailDto`.

### 2. **Updated Repository (`ProductRepository.cs`)**
- `.Include(p => p.Productvariants).ThenInclude(v => v.Inventoryitem)` in `GetProductWithImagesAsync` and `GetBySlugAsync`.

### 3. **Updated Handlers (`GetProductByIdQueryHandler.cs` & `GetProductBySlugQueryHandler.cs`)**
- Mapped `Productvariants` to `variantDtos` with stock count calculation and attached to the response DTO.

---

## 📄 Final Response JSON Structure (`GET /api/products/{id}`)

```json
{
  "productId": "b0000000-0000-0000-0000-000000000011",
  "categoryId": "c0000000-0000-0000-0000-000000000001",
  "name": "Onyx V1 Hoodie",
  "slug": "onyx-v1-hoodie",
  "description": "Áo hoodie thể thao phom rộng Onyx V1 chất liệu cao cấp...",
  "fitType": "Oversized",
  "gender": "Men",
  "isActive": true,
  "createdAt": "2026-08-08T18:28:24.858Z",
  "images": [
    {
      "imageId": "i0000000-0000-0000-0000-000000000001",
      "productId": "b0000000-0000-0000-0000-000000000011",
      "variantId": null,
      "imageUrl": "http://localhost:9000/gymkitten-media/images/2026/08/08/a7c50b08cbb3.jpg",
      "displayOrder": 1,
      "isPrimary": true
    }
  ],
  "variants": [
    {
      "variantId": "a0000000-0000-0000-0000-000000000021",
      "productId": "b0000000-0000-0000-0000-000000000011",
      "sku": "ONX-V1-BLK-M",
      "colorName": "Black",
      "colorHex": "#000000",
      "size": "M",
      "price": 950000,
      "originalPrice": 1100000,
      "weightGrams": 600,
      "available": 10
    },
    {
      "variantId": "a0000000-0000-0000-0000-000000000022",
      "productId": "b0000000-0000-0000-0000-000000000011",
      "sku": "ONX-V1-BLK-L",
      "colorName": "Black",
      "colorHex": "#000000",
      "size": "L",
      "price": 950000,
      "originalPrice": 1100000,
      "weightGrams": 620,
      "available": 5
    }
  ]
}
```

---

## 🧪 Verification
```bash
dotnet build GymKitten_Backend.sln
Build succeeded. 0 Warning(s), 0 Error(s).
```
