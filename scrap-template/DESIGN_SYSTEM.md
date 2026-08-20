# DESIGN SYSTEM — Scrap Template (Thu mua phế liệu)

Tài liệu token thiết kế dùng chung cho toàn bộ public website + admin CMS.
Ngung sự thật duy nhất: `assets/scss/_variables.scss` (compile ra CSS custom properties trong `:root`).

---

## 1. Color

| Token | Giá trị | Dùng cho |
|---|---|---|
| `--c-primary` | `#E4570F` | CTA chính, điểm nhấn, border-left panel, hover nút dark |
| `--c-primary-dark` | `#C2410C` | Hover/pressed nút primary, link hover |
| `--c-primary-darker` | `#9A3412` | Active nút primary |
| `--c-dark` | `#16181C` | Nền section tối, footer, hero, admin sidebar |
| `--c-dark-2` | `#1F2329` | Surface tối phụ (input trên nền tối, hover sidebar) |
| `--c-dark-3` | `#2A2F36` | Border trên nền tối |
| `--c-steel` | `#3E4C59` | Heading phụ, nút secondary, chữ admin |
| `--c-accent` | `#F5B301` | Vàng cảnh báo — hazard stripe, số liệu label trên nền tối, hover link footer |
| `--c-text` | `#1C1F24` | Chữ chính |
| `--c-muted` | `#66707A` | Chữ phụ, mô tả |
| `--c-bg` | `#F6F5F2` | Nền giấy ấm (section xen kẽ) |
| `--c-surface` | `#FFFFFF` | Nền bề mặt (card phẳng, panel, table) |
| `--c-border` | `#E3E1DC` | Border nền sáng — "strong horizontal rules" |
| `--c-success` | `#1E7E34` | Thành công, xu hướng giá tăng |
| `--c-error` | `#C0392B` | Lỗi, xu hướng giá giảm, nút xóa |
| `--c-warning` | `#B45309` | Cảnh báo |
| `--c-info` | `#0C5460` | Trạng thái "Mới" của lead |

**Tỷ lệ sử dụng:** nền sáng là mặc định (paper/surface); nền tối chỉ dùng cho Hero, Stats band, Referral band, Final CTA, Footer, Page hero, Admin sidebar — tạo nhịp "băng tối" xen kẽ trang dài.

**Chữ ký thị giác (anti AI-generic):** vạch **hazard stripe** (cam/vàng xoắn 45°, mixin `hazard-line`) — chỉ dùng tại mép trên Referral band, Final CTA, Page hero brand admin; số outline `-webkit-text-stroke` cho danh sách đánh số.

---

## 2. Typography

| Cấp | Font | Size (clamp) | Ghi chú |
|---|---|---|---|
| Display / H1 | Oswald 600 | `clamp(2.25rem, 1.7rem+2.6vw, 3.75rem)` | uppercase, hero & page hero |
| H2 | Oswald 600 | `clamp(1.75rem, 1.45rem+1.5vw, 2.625rem)` | uppercase, section title |
| H3 | Oswald 600 | `clamp(1.3rem, 1.2rem+.5vw, 1.625rem)` | |
| H4 | Oswald 600 | `1.125rem` | |
| Body | Be Vietnam Pro 400 | `1rem` / line-height 1.7 | |
| Small | Be Vietnam Pro 400 | `.875rem` | |
| Tech label | Oswald 500 | `.75rem`, letter-spacing .22em | uppercase — nhãn kỹ thuật |
| Num display | Oswald 700 | `clamp(2.5rem, 2rem+3vw, 4.5rem)` | số liệu thống kê |
| Price value | Oswald 600 | theo ngữ cảnh | `white-space: nowrap` |

- Tối đa **2 font family** (đủ bảng subset tiếng Việt): Oswald + Be Vietnam Pro.
- Google Fonts: `family=Be+Vietnam+Pro:wght@400;500;600;700&family=Oswald:wght@500;600;700&display=swap`.
- Heading luôn uppercase + tracking nhẹ → cảm giác công nghiệp; body không uppercase.

---

## 3. Spacing

| Token | Desktop | Mobile (<576px) |
|---|---|---|
| `--sec-xl` | 104px | 56px |
| `--sec-lg` (mặc định `.section`) | 80px | 44px |
| `--sec-md` | 56px | — |
| `--sec-sm` | 40px | — |

Gutter editorial: `clamp(1.5rem, 4vw, 4rem)`. Padding component dùng bước Bootstrap 4/8/12/16/24/32px.

---

## 4. Radius (chống AI-style)

| Token | Giá trị | Áp dụng |
|---|---|---|
| `--radius-sm` | 2px | Button, input, select, tag, swatch, pagination, badge |
| `--radius-md` | 4px | Card/panel, ảnh, dropdown, offcanvas item, modal? — modal dùng 4px |
| `--radius-lg` | 6px | Component đặc biệt (hiếm) |

Tuyệt đối không pill, không 20–30px radius.

## 5. Border & Shadow

- Border 1px `--c-border` (sáng) / `--c-dark-3` (tối) — template ưa **đường kẻ rõ** hơn bóng đổ.
- Shadow chỉ 2 mức: `--shadow-sm` (tooltip/subtle), `--shadow-md` (dropdown, header stuck, offcanvas) — không shadow lớn lan tỏa.
- Focus ring: `0 0 0 3px rgba(228,87,15,.22)` mọi control tương tác.

## 6. Grid & Container

- Container tối đa **1280px** (`--container-max`, ghi đè Bootstrap xxl).
- Container hẹp nội dung đọc: `.container-narrow` = 820px (article, SEO content).
- Lưới editorial chuẩn: `.row-editorial` (gutter lớn), hero 5/7, story 5/7, form admin 8/4.
- Mosaic danh mục: `grid` 4 cột × hàng 185px, item span linh hoạt (`tile-xl/lg/md/wide`) — hoạt động với 4/8/12 item.

## 7. Image ratio (chống CLS — bắt buộc width/height)

| Loại | Tỉ lệ | Placeholder |
|---|---|---|
| Hero | 16:10 | `assets/images/hero/hero-main.svg` 1600×1000 |
| Danh mục phế liệu | 4:3 | `assets/images/scrap/*.svg` 1200×900 |
| Dự án | 16:10 | `assets/images/projects/*.svg` 1280×800 |
| Tin tức / Article / Khu vực | 16:9 | `assets/images/news/*.svg`, `locations/*.svg` 1200×675 |
| Công ty | 3:2 | `assets/images/company/*.svg` 1200×800 |

Ảnh dưới fold: `loading="lazy"`. Hero: `fetchpriority="high"`. Toàn bộ ảnh hiện là SVG placeholder có ghi chú ảnh thật cần thay — xem `tools/generate-placeholders.js`.

## 8. Buttons

| Biến thể | Class | Dùng |
|---|---|---|
| Primary | `.btn.btn-primary` | CTA chính: Gửi hình nhận báo giá |
| Dark | `.btn.btn-dark` | Secondary: Tìm hiểu công ty (hover chuyển cam) |
| Outline dark | `.btn.btn-outline-dark` | Hành động phụ nền sáng |
| Light | `.btn.btn-light` / `.btn-outline-light` | Trên nền tối |
| Text link | `.link-arrow` | "Xem tất cả →" |
| Icon | `.icon-btn`, `.ta-actions a` | Bảng admin, chia sẻ |

Size: `.btn-sm` .8125rem / mặc định .9375rem / `.btn-lg` 1.0625rem. Chữ Oswald uppercase, tracking .06em.

## 9. Forms

- Label: `.form-label-tech` (Oswald .8125rem uppercase, required mark cam).
- Control: `.form-control/.form-select` radius 2px, focus cam; state `.is-invalid/.is-valid` + `.invalid-feedback` (kèm icon).
- Upload: `.upload-drop` (drag & drop) + `.upload-previews > .upload-thumb` (preview + remove).
- QuickQuoteForm: `.js-quote-form` — validate VN phone, loading `.quote-state.is-loading` → success/error; fields: Họ tên, SĐT*, Zalo, Loại phế liệu*, Số lượng, Khu vực*, Hình ảnh, Ghi chú.

## 10. Motion

- Reveal on scroll: `.reveal` → `.is-in` (fade + translateY 22px, 550ms) — tôn trọng `prefers-reduced-motion`.
- Hover transform: 2–4px (`translateY(-3px)` card, `translateX(4px)` mũi tên).
- Counter số liệu khi cu tới (`data-count`).
- Cấm: bounce, spin, pulse liên tục, parallax mạnh, scroll hijacking.
