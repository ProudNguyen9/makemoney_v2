# COMPONENTS.md — Thư viện component của Scrap Template

Mỗi component: dùng ở đâu, biến thể, file SCSS nguồn, chú ý tích hợp backend (Razor Partial tương ứng).

Quy ước marker trong HTML: `<!-- COMPONENT NAME START/END -->` và với item lặp: `<!-- SCRAP ITEM START/END -->` — backend biến item lặp thành vòng lặp partial.

---

## PUBLIC COMPONENTS

### TopBar
- **SCSS:** `_header.scss` · **Partial gợi ý:** `Views/Shared/_TopBar.cshtml`
- Dùng: mọi trang public (desktop, ẩn < md).
- Nội dung: note khu vực + email / giờ làm / hotline. Token: `[EMAIL]`, `[GIỜ LÀM VIỆC]`, `[HOTLINE]`.

### Header + DesktopNav
- **SCSS:** `_header.scss` (`.site-header`, `.nav-main`, `.has-drop`) · **Partial:** `_Header.cshtml`
- Sticky top; cuộn > 40px thêm `.is-stuck` (thu nhỏ). Dropdown Bootstrap (hover + click nhờ `navigation.js`).
- Active state: thêm `.active` vào `.nav-link` cấp 1 — **backend render theo route hiện tại**.
- CTA phải: `.header-cta[data-quote-open]` mở Quote Modal.

### MobileNav (Offcanvas) + MobileContactBar
- **SCSS:** `_header.scss` (`.mobile-menu`, `.mobile-bar`) · **Partial:** `_MobileNav.cshtml`, `_MobileBar.cshtml`
- Offcanvas trái `#mobileMenu`; submenu accordion qua `data-sub` (navigation.js), CTA chính nằm cuối offcanvas.
- Mobile bar fixed bottom: GỌI | ZALO | BÁO GIÁ (grid 1fr 1fr 1.4fr); ẩn ≥ 992px. `body.has-mobilebar` chừa padding-bottom tránh che footer.

### PageHero + Breadcrumb
- **SCSS:** `_layout.scss` (`.page-hero`, `.breadcrumb-line`)
- Dùng: mọi trang con (about, prices, news, location…). Ảnh nền mờ `.page-hero-bg` (opacity .18) + h1 + mô tả + breadcrumb trong nền tối.
- Breadcrumb có `[aria-current="page"]` ở item cuối — chuẩn SEO BreadcrumbList.

### SectionHeading (`.sec-head`)
- **Biến thể:** mặc định / `.sec-head--row` (heading trái + link "xem tất cả" phải).
- `.sec-label` (nhãn cam uppercase) + `.sec-title` (H2) + `.sec-desc`.

### TrustStrip
- **SCSS:** `_home.scss`. 4 item kẻ dọc phân cách, KHÔNG card. Dùng: Home, Contact (3 item).

### ScrapCategory mosaic (`.cat-mosaic`)
- **SCSS:** `_home.scss`. Editorial grid 4 cột, item span linh hoạt: `.tile-xl` (2×2), `.tile-lg` (1×2), `.tile-md`, `.tile-wide` (2×1).
- **Chống hard-code dữ liệu:** 4/8/12 item đều hoạt động — item cuối tự lấp ô trống.

### PriceTable (`.table-price`)
- **SCSS:** `_home.scss` (`.price-board`, `.price-trend`), `_price.scss` (`.price-group`).
- **Dùng:** Home, Prices, Scrap category/detail, News sidebar, Location detail, Article.
- **Biến thể:** full (cột xu hướng) / compact (bỏ cột xu hướng).
- Mobile: bọc `.table-scroll` → cuộn ngang, KHÔNG chuyển row thành card.
- Dữ liệu: backend inject từ module Bảng giá; ngày cập nhật `Cập nhật [DD/MM/YYYY]`.

### QuickQuoteForm (`.js-quote-form`)
- **SCSS:** `_forms.scss` (`.quote-panel`, `.upload-drop`, `.quote-state`) · **JS:** `quote-form.js`
- **Dùng:** Quote Modal (mọi trang), Contact, có thể nhúng trang bất kỳ.
- Fields: Họ tên, SĐT*, Zalo, Loại phế liệu*, Số lượng, Khu vực*, Hình ảnh (multi + preview + remove), Ghi chú.
- States: validation inline (SĐT VN), loading (spinner vuông), success, error. Template-only — wire AJAX vào submit handler.

### CompanyStory (`.story-block`)
- **SCSS:** `_home.scss`. Ảnh 3 ô grid (1 dọc cao + 2 ngang) + copy. Dùng: Home, About.

### Statistics (`.stats-band`)
- **SCSS:** `_home.scss`. Nền tối, lưới kẻ ô (1px border), `.num-display` + counter `data-count`.
- **QUAN TRỌNG:** giá trị là placeholder `[10+]`, `[500+]` — backend thay số thật mới đặt `data-count`, không auto đếm token.

### WhyList (numbered)
- **SCSS:** `_home.scss` (`.why-list`). Danh sách 01–04, số outline stroke, kẻ ngang phân cách — thay cho grid card. Dùng: Home, Scrap category, Capability, Service detail.

### ProjectShowcase (`.showcase-grid`) / ProjectRows
- **SCSS:** `_project.scss`. Showcase: 1 lớn (7fr) + 2 nhỏ (5fr) — Home, Capability, Service detail.
- Rows: hàng editorial xen kẽ trái/phải (`.project-row--flip`) + `.project-specs` (khối lượng/thời gian) — trang Projects.

### Process (`.process-steps`)
- **SCSS:** `_home.scss`. 6 bước: ngang desktop (cột + kẻ dọc), dọc mobile. Dùng: Home, Scrap pages, Location detail, Service detail, Referral (5 bước).

### LocationDirectory (`.loc-directory`) / DistrictGrid / IzList
- **SCSS:** `_location.scss`. Nhóm theo tỉnh (heading trái + chips huyện phải, desktop 240px/1fr) — KHÔNG card. Dùng: Home, Locations, Location detail (district grid), About.
- `.iz-list`: danh sách KCN đánh số 01, 02… cho Location detail.

### ArticleFeature / NewsRows / ArticleBody
- **SCSS:** `_article.scss`. News editorial: 1 featured (7fr) + cột rows (5fr) — KHÔNG 4 card giống nhau.
- `.article-body`: nội dung blog max-width đọc thoải mái (`.container-narrow` 820px), h2 viền cam trái, quote-block, bảng giá nhúng, `.article-cta-inline`, `.link-blocks` (internal links), `.article-tags`.

### FAQ (`.faq-accordion`)
- **SCSS:** `_home.scss`. Bootstrap Accordion custom: nút +/− icon, item active viền trái cam. Dùng: Home + mọi trang con.

### ReferralCTA (`.referral-band`) / FinalCTA (`.cta-final`)
- **SCSS:** `_home.scss`. Nền tối + ảnh mờ + hazard stripe mép. Final CTA 3 nút: Gửi hình / Hotline / Zalo.

### FloatingContact + BackTop
- **SCSS:** `_header.scss`, `_base.scss`. Desktop phải màn hình: ZALO / phone / camera — đứng yên, không rung.

### Pagination (`.pagination-industrial`) + FilterBar (`.filter-bar`)
- **SCSS:** `_layout.scss`. Vuông 42px, active nền đen. Filter: chips active nền đen.

---

## ADMIN COMPONENTS

| Component | Class | Partial gợi ý | Ghi chú |
|---|---|---|---|
| AdminSidebar | `.admin-sidebar` | `Admin/_Sidebar.cshtml` | Sticky, nhóm theo mục, submenu `.nav-group-btn`, badge số lead; mobile: overlay + `.at-toggle` |
| AdminTopbar | `.admin-topbar` | `Admin/_Topbar.cshtml` | Search + thông báo + user |
| PageHeader | `.admin-page-head` | `Admin/_PageHeader.cshtml` | h1 + mô tả + `.aph-actions` |
| DataTable | `.table-admin-wrap > .table-admin` | `Admin/_Table.cshtml` | Thumbnail trong `.ta-title`, `.ta-actions` (edit/copy/trash), hover nhạt cam |
| FilterBar | `.admin-filter` | `Admin/_Filter.cshtml` | Select/input nhỏ + spacer + nút |
| StatusBadge | `.badge-status` | `Admin/_StatusBadge.cshtml` | st-new/st-processing/st-done/st-cancel/st-draft/st-published |
| MetricCard | `.metric-card` | `Admin/_Metric.cshtml` | Gạch trái màu (primary/dark/accent/success) |
| Panel | `.admin-panel` | `Admin/_Panel.cshtml` | head/body/foot |
| FormGroup | `.form-label-tech` + control | — | Uppercase Oswald, required mark cam |
| SEOForm | panel "SEO" | `Admin/_SeoForm.cshtml` | SEO title (đếm ký tự), meta 160, canonical, OG, Index/Follow switch |
| Uploader/MediaPicker | `.upload-drop`, `.media-grid` | `Admin/_MediaPicker.cshtml` | Drag&drop, preview, copy URL (JS `data-copy`), grid/list toggle (`data-media-view`) |
| EditorPlaceholder | `.editor-placeholder` | — | Vùng rich editor — wire TinyMCE/CKEditor |
| EmptyState | `.empty-state` | `Admin/_Empty.cshtml` | Icon + tiêu đề + mô tả + nút |
| ConfirmationModal | Bootstrap modal + `.modal-content` viền cam | `Admin/_Confirm.cshtml` | Xóa/đăng xuất |

---

## STATES bắt buộc khi tích hợp

| State | Public | Admin |
|---|---|---|
| Loading | Quote form `.is-loading` | Spinner bảng (thêm sau) |
| Empty | Search "Không tìm thấy nội dung phù hợp" | Lead "Chưa có yêu cầu báo giá" (`.empty-state`) |
| Error | Quote form `.is-error` | `.invalid-feedback`, alert-danger |
| Success | Quote form `.is-success` | Toast, alert-success |
