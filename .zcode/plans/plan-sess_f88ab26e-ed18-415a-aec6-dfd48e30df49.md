# Plan: CRUD Admin (chỉ phần /admin) cho 5 mục + Filter + Xử lý ảnh tự động WebP

## Ngữ cảnh hiện tại
App là **ASP.NET Core MVC (.NET 10) + EF Core + SQL Server** (`ScrapWebsiteLocal`), Razor + Bootstrap 5, JS thường, auth cookie admin. Hiện trạng tại commit `7b931a2`:
- **Bảng giá + Loại phế liệu**: list đọc DB nhưng read-only (nút disabled, footer "Read-only phase"). Toàn bộ admin area **chưa có POST action nào** (trừ Auth login).
- **Dịch vụ / Khu vực / Dự án**: trang admin là HTML tĩnh hardcode; **chưa có model EF, chưa có bảng DB** (schema đích đã có sẵn trong `database/scrap_cms.sql` dòng 271–345).
- **Chưa có upload ảnh** (không có IFormFile nào); bảng `MediaFiles` + model có sẵn nhưng chưa dùng; thư mục `wwwroot/uploads/` trống.
- DB tạo bằng **SQL script theo phase** (không dùng EF migration) — sẽ theo convention này.
- Lưu ý routing: route conventional `admin/{controller}/{action}` **không match action có dấu gạch ngang** → URL POST dùng đúng tên action (VD: `/admin/prices/SaveBulk`).

## Phạm vi: CHỈ phần admin
Không wire trang public (`/dich-vu`, `/khu-vuc`, `/du-an`, `/bang-gia` giữ nguyên như hiện tại). 3 bảng mới bắt đầu trống — nhập liệu qua form admin. Không commit git trừ khi bạn yêu cầu.

## Nguyên tắc UX (đúng yêu cầu "đơn giản sửa tại chỗ, phức tạp làm trang riêng")

| Phần | Cách thao tác | Filter danh sách |
|---|---|---|
| **Bảng giá** | Sửa **trực tiếp trong danh sách**: tick chọn dòng → sửa input giá + đơn vị + switch hiển thị → 1 nút "Lưu bảng giá"; xóa từng dòng; mỗi lần đổi giá **tự ghi vào `ScrapPriceHistory`** | Nhóm + trạng thái + tìm kiếm |
| **Loại phế liệu** | **Trang form riêng** `/admin/scrap/Form/{id?}` (thông tin, sub-table bảng giá, 2 ảnh, cấu hình); danh sách có toggle nhanh nổi bật/hiển thị, sửa thứ tự, xóa | Nhóm + trạng thái + tìm kiếm |
| **Dịch vụ** | **Trang form riêng** (nội dung HTML + ảnh bìa); danh sách có toggle nhanh + sửa thứ tự + xóa (soft delete) | Trạng thái + tìm kiếm |
| **Khu vực** | **Trang form riêng** (tỉnh/quận, nội dung, ảnh, lat/lng); danh sách toggle nhanh + xóa (soft delete) | **Tỉnh** (distinct) + trạng thái + tìm kiếm |
| **Dự án** | **Trang form riêng** (nhiều trường + **gallery multi-upload**); danh sách toggle nhanh + xóa (soft delete) | **Loại dự án** (distinct) + trạng thái + tìm kiếm |

Tất cả danh sách: **phân trang thật 20 dòng/trang** (hiện đang Take(50) không phân trang), partial dùng chung giữ nguyên querystring filter.

---

## Phase A — Nền tảng

### A1. Database + EF models
- Script mới `database/phase3_cms_entities.sql` (idempotent, IF NOT EXISTS): tạo `Services`, `Locations`, `Projects`, `ProjectImages` + index (UX_*_Slug filtered `WHERE DeletedAt IS NULL`, IX_Locations_Province) — schema copy từ `scrap_cms.sql`. Chạy trên `ScrapWebsiteLocal` bằng sqlcmd.
- Models mới `codezone/Models/`: `Service.cs`, `Location.cs`, `Project.cs`, `ProjectImage.cs` (soft delete qua `DeletedAt`).
- `AppDbContext`: thêm 4 DbSet + mapping (Latitude/Longitude `decimal(10,7)`, Project→ProjectImages cascade, max-length khớp bảng).

### A2. Xử lý ảnh tự động → WebP (yêu cầu chính)
- Package: **`SixLabors.ImageSharp`** (pure .NET, encode WebP tốt, chuẩn cho ASP.NET Core; Split License free cho doanh nghiệp nhỏ).
- `codezone/Services/Media/ImageUploadService.cs` (`IImageUploadService`):
  - **Validate**: chỉ nhận JPG/PNG/WebP/GIF, tối đa 10MB, kiểm tra magic bytes bằng chính ImageSharp decode (tệp đổi đuôi sai định dạng vẫn bị chặn), chặn SVG.
  - **Pipeline**: decode → AutoOrient (xoay theo EXIF) → resize chỉ khi rộng hơn max (giữ tỷ lệ) → **encode WebP quality 80** → lưu `wwwroot/uploads/{folder}/{yyyyMM}/{slug}-{random}.webp`. **Không giữ file gốc** → giảm dung lượng ổ đĩa.
  - Ghi row `MediaFiles` (tên gốc, url, folder, `image/webp`); hàm xóa file cũ + row khi ảnh bị thay/xóa (chỉ đụng file trong `/uploads/`, có guard path traversal).
  - Max width theo loại: thumb 800 / ảnh bìa 1200 / banner & gallery 1600; config `Media:Quality`, `Media:MaxUploadBytes` trong `appsettings.json`.
- Upload dạng **post kèm form** (IFormFile trong view model) — không phụ thuộc JS, hoạt động kể cả JS tắt.
- JS nhỏ trong `admin.js`: preview tên/ảnh khi chọn file trong `.upload-drop`.

### A3. Tầng write + tiện ích chung
- `Services/Admin/AdminCommandService.cs` + file interfaces riêng (`IAdminPriceCommandService`, `IAdminScrapCommandService`, `IAdminServiceCommandService`, `IAdminLocationCommandService`, `IAdminProjectCommandService`) — theo pattern "1 class nhiều interface" như `AdminQueryService` hiện có; đăng ký DI tương ứng.
- Chung: sinh slug (SlugHelper có sẵn) + tự unique (thêm `-2`, `-3`); UpdatedAt/PublishedAt; soft delete cho service/location/project, hard delete cho scrap item; mọi POST có anti-forgery; redirect sau khi lưu (PRG); lỗi upload hiển thị qua flash message.
- **Flash message**: TempData + partial `_Alerts.cshtml` trên `_AdminLayout`.
- **TinyMCE qua CDN** cho các trường nội dung (class `.js-rich-editor`, chặn thẻ script/iframe, init trong `_AdminScripts`) — thay partial editor giả hiện tại.
- JS `admin.js` thêm: `data-autosubmit` (switch đổi là tự submit form), `data-row-remove` (xóa dòng sub-table).
- Partial `_AdminPagination.cshtml` + `AdminPaginationViewModel`.

### A4. Mở rộng tầng query
- `AdminQueryService` + interfaces: thêm `page` cho `GetScrapListAsync`/`GetPriceListAsync`; thêm `status` filter cho giá; thêm `GetCategoryOptionsAsync`, `GetScrapFormAsync(id)`; mới `IAdminServiceQueryService`, `IAdminLocationQueryService`, `IAdminProjectQueryService` (list + form loader).
- ViewModels mới trong `AdminDataViewModels.cs`: row DTO + list VM cho 3 entity mới (kèm `Pager` tính sẵn), `AdminPriceRowDto` thêm `ScrapItemId` + `ItemIsPublished`.

---

## Phase B — CRUD từng phần

### B1. Bảng giá — sửa trực tiếp tại danh sách (`PricesController` + `Views/Prices/Index.cshtml`)
- Bảng nằm trong 1 form POST `/admin/prices/SaveBulk`: cột checkbox (check-all, JS `data-check-all` có sẵn) + hidden `rows[i].PriceId/ScrapItemId` + input giá (bỏ disabled) + select đơn vị (kg/tấn/cái/m/lô) + **switch hiển thị** (= bật/tắt trạng thái của loại phế liệu cha, lưu trong cùng bulk save).
- Chỉ dòng được tick mới lưu. Đổi giá → update `ScrapPrice` + insert `ScrapPriceHistory` (giá mới, đơn vị, Note "Cập nhật từ bảng giá quản trị") + đồng bộ `ScrapItem.PriceFrom` = giá thấp nhất khi item không có PriceLabel text.
- Xóa dòng: nút trong dòng, submit bằng `formaction` (POST `/admin/prices/Delete/{id}`).
- Nút "Thêm dòng giá" → dẫn về `/admin/scrap` (dòng giá mới thêm trong sub-table của form loại phế liệu).

### B2. Loại phế liệu — trang form riêng (`ScrapController` + `Views/Scrap/Form.cshtml` build từ prototype)
- GET `Form(int? id)` tạo/sửa; POST `Save` (multipart) với `ScrapItemFormViewModel`: nhóm (select từ DB), tên (bắt buộc), slug (tự sinh từ tên bằng JS + server fallback), mô tả ngắn, nội dung (TinyMCE), giá tham khảo text, đơn vị.
- **Sub-table "Bảng giá áp dụng"**: mỗi dòng = 1 phân loại → 1 `ScrapPrice` (PriceLabel = tên phân loại, PriceValue = giá, Unit); JS "Thêm phân loại" / xóa dòng, dùng hidden `PriceRows.Index` để binding không cần index liên tục; lưu = replace toàn bộ dòng giá cũ của item.
- **Ảnh**: ảnh đại diện → `PrimaryImage` (max 800px); ảnh banner → row `ScrapImages` với `Caption = "banner"` (max 1600px); hiện ảnh hiện tại + checkbox xóa ảnh; thay ảnh mới = tự xóa file WebP cũ.
- Cấu hình: nổi bật, trạng thái, thứ tự. Link "Xem trang công khai" khi sửa.
- **Index**: bật nút Sửa (→ `/admin/scrap/Form/{id}`), Xóa (confirm, xóa kèm giá + ảnh + file), switch nổi bật/hiển thị (autosubmit), input thứ tự + nút lưu; filter nhóm/trạng thái/q + phân trang.

### B3. Dịch vụ — thay 2 view tĩnh (`ServicesController` + Views)
- Index: DB list (lọc theo `DeletedAt == null`), filter trạng thái + tìm kiếm, toggle nhanh, sửa thứ tự, xóa = soft delete, nút Thêm.
- Form `/admin/services/Form/{id?}`: tiêu đề (bắt buộc), slug tự sinh, icon CSS (bootstrap-icons), mô tả ngắn, nội dung (TinyMCE), ảnh bìa (upload → WebP 1200px), trạng thái, nổi bật, thứ tự.

### B4. Khu vực — thay 2 view tĩnh (`LocationsController` + Views)
- Index: DB list, filter theo **tỉnh** (distinct Province) + trạng thái + tìm kiếm (tên/slug/tỉnh), toggle nhanh, soft delete.
- Form: tỉnh (bắt buộc), quận/huyện, tên (bắt buộc), slug tự sinh, mô tả ngắn, nội dung (TinyMCE), ảnh bìa (WebP), vĩ độ/kinh độ (validate -90..90 / -180..180), trạng thái, nổi bật, thứ tự.

### B5. Dự án — thay 2 view tĩnh (`ProjectsController` + Views)
- Index: DB list, filter theo **loại dự án** (distinct ProjectType) + trạng thái + tìm kiếm, toggle nhanh, soft delete.
- Form: tiêu đề (bắt buộc), slug, loại dự án, địa điểm, mô tả ngắn, nội dung (TinyMCE), ảnh bìa (WebP), ngày hoàn thành, sản lượng, thời gian thi công, **gallery**: multi-upload (mỗi file → 1 WebP riêng 1600px), danh sách ảnh hiện tại có alt text + thứ tự + xóa từng ảnh.

---

## Phase C — Kiểm thử
- `dotnet build` sạch; chạy script SQL; chạy app (`dotnet run`).
- **GUI test bằng browser** (đăng nhập `admin@phelieuthanhtrung.vn / Admin@2026!`):
  - CRUD đủ 5 phần: thêm/sửa/xóa, toggle, sort, filter, phân trang.
  - **Upload ảnh JPG/PNG → xác nhận file `.webp` xuất hiện trong `wwwroot/uploads/` với dung lượng giảm**; upload file sai định dạng/qua lớn → báo lỗi.
  - Sửa giá trong bảng giá → kiểm tra `ScrapPriceHistory` có dòng mới.
  - Trang public không bị thay đổi (chỉ đọc admin).

## Thứ tự implement
A1 → A2 → A3 → A4 → B1 → B2 → B3 → B4 → B5 → C (mỗi list làm pagination/filter ngay trong phần của nó)