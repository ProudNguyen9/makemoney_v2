# SCRAP TEMPLATE — Bộ Frontend Template Website Thu Mua Phế Liệu

Template HTML tĩnh hoàn chỉnh (public + admin CMS) cho doanh nghiệp **thu mua phế liệu**, sẵn sàng bàn cho Backend Developer tích hợp **ASP.NET Core MVC** (hoặc Laravel/PHP/CMS).

- **Design direction:** Industrial + Corporate + Modern + Professional + High Conversion
- **Stack:** HTML5 semantic · Bootstrap 5.3.3 (local) · SCSS → CSS · Vanilla JS · Bootstrap Icons · Swiper 11 (chỉ gallery) · Không React/Vue/Angular
- **CSS/JS tách file riêng** — không inline style, không inline script (JSON-LD backend inject là ngoại lệ chuẩn)

---

## 1. Chạy dự án

Mở trực tiếp `index.html`, hoặc chạy static server (khuyên dùng để test đầy đủ):

```bash
cd scrap-template
npx serve .        # hoặc: python -m http.server 8080 / php -S localhost:8080
```

Admin: mở `admin/login.html` → `admin/dashboard.html` (template only, chưa có backend).

## 2. Build SCSS

Yêu cầu Node ≥ 18. Lần đầu: `npm install`.

```bash
npm run build   # main.css + admin.css + responsive.css
npm run watch   # theo dõi thay đổi main.scss/admin.scss
```

Nguồn SCSS tại `assets/scss/`, xuất ra `assets/css/`. Token thiết kế duy nhất: `_variables.scss`.

## 3. Cấu trúc thư mục

```text
scrap-template/
├── index.html                # Trang chủ (flagship)
├── about.html                # Giới thiệu
├── capability.html           # Năng lực (B2B)
├── scrap.html                # Danh sách phế liệu
├── scrap-category.html       # Danh mục (VD: phế liệu đồng)
├── scrap-detail.html         # Chi tiết loại (VD: đồng đỏ)
├── services.html             # Danh sách dịch vụ
├── service-detail.html       # Chi tiết dịch vụ (nhà xưởng)
├── prices.html               # Bảng giá (SEO trọng điểm)
├── locations.html            # Danh sách khu vực
├── location-detail.html      # Landing khu vực (Đồng Nai)
├── projects.html             # Dự án
├── project-detail.html       # Chi tiết dự án (Swiper gallery)
├── news.html                 # Tin tức (editorial)
├── article.html              # Chi tiết bài viết
├── referral.html             # Chính sách hoa hồng
├── contact.html              # Liên hệ + Quick Quote
├── search.html               # Tìm kiếm
├── 404.html                  # Không tìm thấy
├── ui-kit.html               # UI Kit / Design System demo
│
├── partials/                 # Mẫu chrome dùng chung → convert Razor Partial
│   ├── _header.html          #   _Header.cshtml (TopBar+Header+Nav+Offcanvas)
│   ├── _footer.html          #   _Footer.cshtml (+Floating+MobileBar+QuoteModal)
│   ├── _scripts.html         #   section Scripts của _Layout
│   └── _admin-chrome.html    #   Admin/_Layout.cshtml (Sidebar+Topbar)
│
├── admin/                    # CMS (Bootstrap + admin.css, body.admin-body)
│   ├── login.html  dashboard.html
│   ├── leads/ (index, detail)      ├── scrap/ (index, form)
│   ├── prices/index.html           ├── services/ (index, form)
│   ├── locations/ (index, form)    ├── articles/ (index, form)
│   ├── projects/ (index, form)     ├── faq/ (index, form)
│   ├── media/index.html            ├── homepage/index.html
│   ├── menu/index.html             ├── seo/index.html
│   └── settings/index.html
│
├── assets/
│   ├── css/ (main.css, admin.css, responsive.css)
│   ├── scss/ (_variables, _mixins, _base, _typography, _layout, _buttons,
│   │          _forms, _header, _footer, _home, _scrap, _price, _article,
│   │          _location, _project, _page + main/admin/responsive)
│   ├── js/ (main.js, navigation.js, quote-form.js, gallery.js, admin.js)
│   ├── vendor/ (bootstrap.min.css/js, bootstrap-icons, swiper-bundle)
│   └── images/ (logo, hero, scrap, company, projects, news, locations)
├── tools/generate-placeholders.js   # Tạo lại ảnh placeholder SVG
├── DESIGN_SYSTEM.md          # Token: màu, chữ, spacing, radius, ảnh…
├── COMPONENTS.md             # Danh mục component + biến thể + partial map
└── README.md
```

## 4. JS dependencies

| File | Nạp ở | Chức năng |
|---|---|---|
| `vendor/bootstrap.min.js` | mọi trang | Dropdown, offcanvas, modal, accordion, toast |
| `js/main.js` | public | Sticky header, reveal, counter, back-top, anchor nav |
| `js/navigation.js` | public | Dropdown hover, offcanvas submenu, active link |
| `js/quote-form.js` | public | QuickQuoteForm: validate, upload preview, states |
| `vendor/swiper-bundle.min.js` + `js/gallery.js` | **chỉ** project-detail | Gallery |

## 5. Placeholder cần thay trước khi lên production

**Thông tin doanh nghiệp** (tìm–thay toàn bộ `[...]`):

```text
[TÊN CÔNG TY] [MÃ SỐ THUẾ] [HOTLINE] [ZALO] [EMAIL] [ĐỊA CHỈ] [ĐỊA CHỈ KHO]
[GIỜ LÀM VIỆC] [30 phút] [10+] [500+] [20XX] [QUẢN TRỊ VIÊN] [DD/MM/YYYY]
```

**Ảnh:** toàn bộ `.svg` trong `assets/images/` là placeholder đúng tỉ lệ (16:10 / 4:3 / 16:9 / 3:2), có ghi chú ảnh thật cần thay ngay trên ảnh. Thay bằng `.webp` cùng tên+kích thước (giữ `width/height` trong HTML để tránh CLS). Danh sách chi tiết: `tools/generate-placeholders.js`.

**Bảng giá** là giá tham khảo thị trường — thay bằng dữ liệu thật từ admin.

## 6. Backend integration (ASP.NET Core MVC)

- **Layout:** partials/ tương ứng 1–1 với `_Layout.cshtml` (public) và `Admin/_Layout.cshtml`. Mỗi trang HTML là mẫu cho 1 View: phần `<main>` giữa header và footer chính là body view.
- **Marker:** mọi khối lặp có `<!-- ... ITEM START/END -->`; section có `<!-- SECTION ... START/END -->`.
- **Token `[...]`:** thay bằng `@Model`/`ViewData`/appsettings (vd: hotline từ `CompanySettings`).
- **SEO:** mỗi view chỉ cần gán title/meta/OG/canonical; canonical hiện là `https://example.com/...`.
- **Structured data:** vị trí comment sẵn trong `<head>` — inject JSON-LD `Organization`, `LocalBusiness`, `BreadcrumbList`, `Article`, `Service`, `FAQPage` theo trang (không hardcode dữ liệu giả).
- **Form báo giá:** mọi `.js-quote-form` dùng chung validator — wire AJAX vào 1 handler duy nhất trong `quote-form.js` (đang demo success state).
- **Anti-pattern đã tránh:** không shopping cart/checkout (business là ĐI MUA), không hard-code layout theo số item (mosaic/grid hoạt động 4/8/12 item).

## 7. Responsive đã kiểm thử theo thiết kế

Breakpoint: ≥1400 / 1200–1399 / 992–1199 (nav rút gọn, ẩn hotline) / 768–991 / 576–767 / <576 (CTA full-width, mobile bar fixed). Bảng giá cuộn ngang, process xoay dọc, hero stack.

Kiểm tra thủ công các cỡ: 1440×900 · 1366×768 · 1024×768 · 768×1024 · 430×932 · 390×844 · 375×667.

## 8. Trình duyệt

Chrome, Edge, Firefox, Safari hiện đại. Không hỗ trợ IE.
