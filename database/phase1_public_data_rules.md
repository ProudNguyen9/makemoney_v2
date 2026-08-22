# Rule Đổ Dữ Liệu Public Phase 1

## Mục Tiêu

Phase 1 chỉ đổ dữ liệu phục vụ client/public, ưu tiên trang chủ và các trang danh sách/detail đã có:

- Trang chủ `/`
- Phế liệu `/phe-lieu`, `/phe-lieu/{slug}`
- Tin tức `/tin-tuc`, `/tin-tuc/{slug}`
- Header/footer public
- SEO public

Không làm admin CRUD, upload, auth, dashboard trong phase này.

## Nguyên Tắc Chung

- Database chỉ lưu dữ liệu động cần quản trị hoặc dễ thay đổi.
- View/Razor giữ layout, markup, class CSS, hiệu ứng, section tĩnh.
- Không đưa toàn bộ nội dung template tĩnh vào database.
- Không lưu Windows path trong database.
- Database chỉ lưu public URL dùng được trên web, ví dụ:
  `/assets/images/imported/products/thumuadongdo1.jpg`
- File ảnh thật phải nằm trong:
  `codezone/wwwroot/assets/images/...`
- Nếu dữ liệu database thiếu ảnh, lấy ảnh có sẵn trong `wwwroot` để seed tạm cho test.
- Không để placeholder như `[HOTLINE]`, `[ZALO]`, `[DD/MM/YYYY]`, `[TÊN CÔNG TY]` trên public HTML.
- Sau này admin có thể thay ảnh/text bằng cách sửa các record/key tương ứng trong DB.

## Rule Riêng Cho Ảnh

Nguồn ưu tiên khi render ảnh public:

1.  Link ảnh trong database.
2.  Nếu DB null/rỗng, dùng fallback ảnh có thật trong `wwwroot`.
3.  Nếu fallback được dùng thường xuyên, thêm key/field vào DB để admin đổi sau.

Không làm:

- Không hard-code ảnh chính trong view nếu ảnh đó là nội dung public cần thay đổi.
- Không lưu `C:\...`, `D:\...`, hoặc path local máy vào database.
- Không trỏ đến file chưa tồn tại trong `wwwroot`.
- Không copy toàn bộ thư mục ảnh nếu chỉ dùng một phần nhỏ cho test.

## Mapping Ảnh Theo Section

### Header/Footer

- Logo header: `SiteSettings.brand.logo`
- Logo footer: `SiteSettings.brand.logo_footer`
- Favicon: `SiteSettings.brand.favicon`
- Apple touch icon: `SiteSettings.brand.apple_touch_icon`

Ảnh nằm trong:

`/assets/images/imported/brand/...`

### Trang Chủ Hero

- Ảnh chính hero: `Banners.ImageUrl`
- Ảnh hero phụ:
  - `SiteSettings.brand.banner_1`
  - `SiteSettings.brand.banner_2`
  - `SiteSettings.brand.banner_3`

Nếu thiếu, fallback:

- `/assets/images/imported/brand/banner-1.jpg`
- `/assets/images/imported/brand/banner-2.jpg`
- `/assets/images/imported/brand/banner-3.jpg`

### Trang Chủ Phế Liệu

- Ảnh tile/category/product lấy từ `ScrapItems.PrimaryImage`.
- Fallback lấy từ `/assets/images/imported/products/...`.
- Chỉ lấy các item public: `Status = 'published'`.
- Query home giới hạn nhỏ, hiện dùng tối đa 8 item.

### Trang Chủ Bảng Giá

- Dữ liệu bảng giá lấy từ `ScrapItems` đã query cho home.
- Không hard-code giá trong view.
- Ngày cập nhật lấy từ `SiteSettings.home.price_updated_text`.

### Trang Chủ Về Chúng Tôi

Các ảnh section lấy từ `SiteSettings`:

- `home.about_image_main`
- `home.about_image_truck`
- `home.about_image_scale`

Fallback hiện dùng ảnh brand/banner có thật trong `wwwroot`.

### Trang Chủ Dự Án Gần Đây

Phase 1 chưa có bảng project động riêng, nên ảnh test lấy từ `SiteSettings`:

- `home.project_image_1`
- `home.project_image_2`
- `home.project_image_3`

Khi làm admin/project thật sau này, chuyển sang bảng project riêng.

### Trang Chủ Khu Vực

- Danh sách khu vực lấy từ `SiteSettings.contact.purchase_areas`.
- Tách bằng dấu phẩy.
- Không cần bảng riêng trong Phase 1 nếu chỉ là danh sách marketing ngắn.

### Trang Chủ Hoa Hồng

- Ảnh nền section hoa hồng: `SiteSettings.home.referral_image`.
- Text hotline/Zalo lấy từ:
  - `SiteSettings.contact.phone`
  - `SiteSettings.contact.zalo`

### Trang Chủ Final CTA

- Ảnh nền CTA cuối: `SiteSettings.home.final_cta_image`.
- Hotline/Zalo lấy từ DB, không hard-code.

### Tin Tức Trang Chủ

- Bài chính bên trái: item đầu tiên trong `Posts`.
- 5 bài bên phải: `latestPosts.Skip(1).Take(5)`.
- Ảnh bài viết lấy từ `Posts.CoverImage`.
- Nếu thiếu ảnh, fallback:
  `/assets/images/imported/brand/seo-og-image.png`
- Query home lấy tối đa 6 bài.

### Detail Phế Liệu

- Ảnh chính: `ScrapItems.PrimaryImage`.
- Gallery: `ScrapItemImages`, chỉ lấy số lượng nhỏ cần dùng.
- Không dùng `Include` cho public query; dùng projection DTO.

### Detail Tin Tức

- Trang detail không render ảnh cover lớn riêng ở đầu bài để tránh trùng và làm trang quá cao.
- Ảnh trong nội dung bài viết đến từ `Posts.ContentHtml`, nhưng mọi `src` phải là public URL tồn tại trong `wwwroot`.
- Không giữ đường dẫn template cũ như `../assets/images/blogs/inline/...` trong `Posts.ContentHtml`.
- Gallery bổ sung lấy từ `PostImages`, query riêng và giới hạn nhỏ.
- SEO OG image ưu tiên ảnh cover.

## Rule Cho Contact/Header/Footer

Các thông tin contact public lấy từ `SiteSettings`:

- `site.name`
- `contact.phone`
- `contact.email`
- `contact.zalo`
- `contact.address`
- `contact.working_hours`
- `contact.warehouse_address`
- `company.tax_code`
- `contact.purchase_areas`

Render href:

- Phone: chỉ lấy chữ số, render `tel:{digits}`.
- Zalo: nếu là URL thì dùng nguyên URL; nếu là số thì render `https://zalo.me/{digits}`.

## Rule Query Hiệu Suất

- Public query luôn dùng `AsNoTracking()`.
- Public query dùng `Select(...)` projection sang DTO/read model.
- Không trả EF entity trực tiếp ra view.
- Không dùng `Include(...)` cho public query Phase 1.
- Không dùng `Skip(...)` cho list pagination public; dùng cursor pagination ở list.
- Home chỉ lấy số lượng nhỏ:
  - Phế liệu: tối đa 8.
  - Tin tức: tối đa 6.
  - Banner: 1 active banner chính.
- Detail gallery lấy riêng và giới hạn số ảnh.

## Rule Seed/Fallback SQL

Khi thêm ảnh/text test:

- Dùng script SQL riêng, idempotent.
- Dùng `MERGE` cho `SiteSettings`/`MediaFiles` nếu cần upsert.
- Script không overwrite giá trị đã có nếu admin/dev đã sửa thủ công, trừ khi thật sự cần fix path gãy.
- Các script hiện có:
  - `phase1_brand_assets.sql`
  - `phase1_home_settings.sql`
  - `phase1_home_image_fallbacks.sql`
  - `phase1_site_chrome_settings.sql`

## Checklist Sau Khi Đổ Dữ Liệu

- `dotnet build` phải thành công.
- Trang `/` không còn placeholder dạng `[HOTLINE]`, `[ZALO]`, `[DD/MM/YYYY]`.
- HTML public dùng ảnh `/assets/images/imported/...` hoặc ảnh có thật trong `wwwroot`.
- Mở trực tiếp vài URL ảnh phải trả `200`.
- DB không có path Windows.
- Các query public không có `Include(` hoặc `Skip(` trong service Phase 1.
- Dữ liệu test không vượt phạm vi Phase 1.
