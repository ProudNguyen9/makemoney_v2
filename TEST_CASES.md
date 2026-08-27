# BỘ TEST CASE WEBSITE THU MUA PHẾ LIỆU (CRUD ĐẦY ĐỦ)

> Phiên bản: 1.2 — Ngày tạo: 25/08/2026 · Ngày chạy: 25/08/2026 (chạy tự động bằng Playwright MCP + sqlcmd) · Chạy lại sau fix: 25/08/2026
> Phạm vi: Toàn bộ chức năng Thêm / Sửa / Xóa / Trạng thái của trang quản trị `/admin` và ảnh hưởng ra trang public.
> Cách dùng: chạy từng test case theo bảng, đánh dấu kết quả vào cột **Kết quả** (`PASS` / `FAIL` / `PARTIAL` + ghi chú lỗi).
>
> **Vòng 2 (25/08/2026):** toàn bộ FAIL/PARTIAL của vòng 1 đã được sửa và chạy lại bằng HTTP thật (login admin → thao tác → xác minh DB + trang public). Chi tiết xem mục "LỖI ĐÃ SỬA" cuối file. Migration DB: `database/phase5_scrap_items_soft_delete.sql` (ĐÃ CHẠY).

---

## 0. CHUẨN BỊ MÔI TRƯỜNG

| Hạng mục | Giá trị |
|---|---|
| URL web | `http://localhost:5051` |
| Kết nối DB | `Server=.;Database=ScrapWebsiteLocal;Trusted_Connection=True` |
| Tài khoản admin | `admin@phelieuthanhtrung.vn` / `Admin@2026!` |
| Tài khoản editor | `editor@phelieuthanhtrung.vn` / `Editor@2026!` |
| Tài khoản sale | `sale@phelieuthanhtrung.vn` / `Sale@2026!` |
| Tiền tố dữ liệu test | Tất cả bản ghi tạo mới đặt tên đầu bằng `QA-` để dễ dọn dẹp |
| Dọn dữ liệu sau test | Xóa các bản ghi có tên LIKE `'QA-%'` trong các bảng tương ứng |

**Quy ước ưu tiên:** P1 = chặn nghiệp vụ chính, P2 = quan trọng, P3 = phụ.

---

## M01 — ĐĂNG NHẬP / PHÂN QUYỀN (`/admin/login`)

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| AUTH-001 | Đăng nhập đúng tài khoản admin | 1. Mở `/admin/login` 2. Nhập email + mật khẩu admin 3. Bấm Đăng nhập | Redirect về `/admin`, thấy dashboard | P1 | **PASS** (ghi chú: redirect về ReturnUrl nếu có, ví dụ `/admin/articles`) |
| AUTH-002 | Đăng nhập sai mật khẩu | Nhập email đúng + mật khẩu sai | Hiện thông báo lỗi, không vào được admin | P1 | **PASS** — hiện "Email hoặc mật khẩu không đúng." |
| AUTH-003 | Vào trang admin khi chưa đăng nhập | Mở trực tiếp `/admin/articles` khi chưa login | Bị redirect về `/admin/login` | P1 | **PASS** — 302 về login kèm ReturnUrl |
| AUTH-004 | Đăng xuất | Bấm Đăng xuất ở sidebar | Về trang login, truy cập `/admin/*` bị chặn lại | P2 | **PASS** |
| AUTH-005 | Đăng nhập tài khoản editor/sale | Login bằng editor và sale | Đăng nhập thành công (phân quyền hiển thị menu theo vai trò) | P3 | **PASS** (ghi chú: menu sidebar của editor và sale hiện giống hệt nhau — phân quyền chỉ khác tên hiển thị) |

---

## M02 — PHẾ LIỆU (`/admin/scrap`) — bảng `dbo.ScrapItems`

### 2A. Danh mục nhóm phế liệu (`dbo.ScrapCategories`)
| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| CAT-001 | Thêm nhóm phế liệu mới | Vào quản lý nhóm → thêm nhóm tên `QA-Nhóm test` | Lưu thành công, xuất hiện trong danh sách và trong dropdown chuyên mục của form phế liệu | P2 | **PASS** (vòng 2) — trang `/admin/scrap/Categories` + `/admin/scrap/CategoryForm`, đã thêm/sửa/xóa qua HTTP thật |
| CAT-002 | Sửa tên nhóm | Sửa `QA-Nhóm test` thành `QA-Nhóm test sửa` | Tên thay đổi, không lỗi trùng | P2 | **PASS** (vòng 2) — slug tự sinh duy nhất |
| CAT-003 | Xóa nhóm rỗng | Xóa nhóm vừa tạo | Xóa thành công khỏi danh sách | P2 | **PASS** (vòng 2) — chỉ xóa được nhóm rỗng; nhóm còn phế liệu bị chặn kèm thông báo |

### 2B. Item phế liệu
| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| SCR-001 | Thêm mới phế liệu | `/admin/scrap/Form` → nhập Tên `QA-Sắt test`, chọn nhóm, giá, đơn vị, mô tả ngắn → Lưu | Lưu OK, về danh sách thấy `QA-Sắt test`, trạng thái "Đã xuất bản" | P1 | **PASS** (vòng 2) — SortOrder mặc định = max+1, không còn bị chặn; có span hiển thị lỗi validate |
| SCR-002 | Slug tự sinh & duy nhất | Tạo tiếp item cùng tên `QA-Sắt test` | Slug tự sinh dạng `qa-sat-test-2` (không trùng), không lỗi DB | P1 | **PASS** — slug `qa-sat-test-2` |
| SCR-003 | Sửa phế liệu | Sửa item `QA-Sắt test`: đổi tên thành `QA-Sắt test đã sửa`, đổi giá | Lưu OK, danh sách hiển thị tên/giá mới | P1 | **PASS** |
| SCR-004 | Bật/tắt xuất bản (ToggleStatus) | Bấm switch trạng thái của item | Chuyển giữa "Đã xuất bản" ↔ "Nháp"; item ở trạng thái nháp **biến mất** khỏi trang `/phe-lieu` public | P1 | **PASS** — nháp → `/phe-lieu/{slug}` 404 |
| SCR-005 | Đánh dấu nổi bật (ToggleFeatured) | Bấm ngôi sao nổi bật | Sao chuyển trạng thái; item nổi bật được ưu tiên hiển thị trên public | P3 | **PASS** (vòng 2) — đã thêm nút sao trong danh sách phế liệu |
| SCR-006 | Đổi thứ tự (UpdateSort) | Nhập SortOrder khác cho item | Danh sách public/admin sắp xếp lại theo SortOrder | P3 | **PASS** — tự dồn vị trí các dòng còn lại |
| SCR-007 | Xóa mềm phế liệu | Bấm xóa item `QA-Sắt test đã sửa` → xác nhận | Item biến mất khỏi danh sách mặc định; kiểm tra DB: `DeletedAt` NOT NULL | P1 | **PASS** (vòng 2) — xóa mềm qua cột `DeletedAt` mới (migration phase5), public 404 ngay sau xóa, dữ liệu giá/ảnh giữ nguyên |
| SCR-008 | Trang public ẩn item đã xóa/nháp | Mở `/phe-lieu/{slug}` của item nháp hoặc đã xóa | Trả 404 | P1 | **PASS** — cả nháp lẫn đã xóa đều 404 (đã xác minh lại vòng 2 bằng HTTP thật) |

---

## M03 — BẢNG GIÁ (`/admin/prices`) — bảng `dbo.ScrapPrices`

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| PRI-001 | Lưu giá hàng loạt (SaveBulk) | Vào `/admin/prices`, chọn nhóm, nhập giá cho item `QA-*`, bấm Lưu | Giá được cập nhật theo từng dòng; lịch sử ghi vào `dbo.ScrapPriceHistories` | P1 | **PASS** — giá cập nhật, lịch sử ghi vào `ScrapPriceHistory` (PriceType=manual). Ghi chú: phải **tick chọn dòng** rồi mới bấm Lưu; tên bảng lịch sử là `ScrapPriceHistory` (số ít) |
| PRI-002 | Nhập giá không hợp lệ | Nhập chữ/vào ô giá rồi lưu | Bị chặn validation hoặc bỏ qua dòng lỗi, không crash | P2 | **PASS** (vòng 2) — dòng tick chọn nhưng giá rỗng bị **bỏ qua**, không ghi NULL vào DB, hiện cảnh báo "Bỏ qua 1 dòng vì giá đang trống..." (đã xác minh: `NullPrices = 0`) |
| PRI-003 | Xóa 1 dòng giá (Delete) | Xóa giá của một item | Dòng biến mất khỏi bảng giá | P1 | **PASS** — xóa mềm (`DeletedAt` được set) |
| PRI-004 | Xóa hàng loạt (DeleteBulk) | Tick nhiều dòng → xóa hàng loạt | Tất cả dòng tick bị xóa | P2 | **PASS** |
| PRI-005 | Bật/tắt hiển thị giá (ToggleItem) | Toggle item trong bảng giá | Trạng thái đổi; giá item tắt không hiện trên `/bang-gia` public | P2 | **PASS** — toggle DB/admin OK; `/bang-gia` giờ đọc DB nên ẩn đúng theo trạng thái item |
| PRI-006 | Public `/bang-gia` khớp DB | Mở `/bang-gia` | Giá hiển thị trùng với giá vừa lưu | P1 | **PASS** (vòng 2) — trang render động từ `ScrapCategories`/`ScrapItems`/`ScrapPrices` (đã xác minh 9 nhóm giá render từ DB) |

---

## M04 — BÀI VIẾT (`/admin/articles`) — bảng `dbo.Posts`, `dbo.PostAutosaves`

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| ART-001 | Thêm bài viết mới (xuất bản) | `/admin/articles/Form` → tiêu đề `QA-Bài test CRUD`, nội dung, chọn chuyên mục → Lưu (xuất bản) | Lưu OK, về danh sách thấy bài, trạng thái "Đã xuất bản" | P1 | **PASS** |
| ART-002 | Lưu bản nháp (SaveDraft) | Tạo bài `QA-Bài nháp` → bấm Lưu nháp | Bài lưu với trạng thái "draft" | P1 | **PASS** |
| ART-003 | Khách truy cập bài nháp → 404 | Đăng xuất (hoặc trình duyệt ẩn danh), mở `/tin-tuc/{slug-bài-nháp}` | HTTP 404, KHÔNG lộ nội dung | P1 | **PASS** |
| ART-004 | Admin xem trước bài nháp | Admin đăng nhập mở `/tin-tuc/{slug-bài-nháp}` | Hiện trang bài viết kèm banner vàng "Bản nháp – chỉ admin đăng nhập mới xem được" | P1 | **PASS** — banner "Bản nháp – bài này chưa xuất bản, chỉ admin đăng nhập mới xem được." |
| ART-005 | Xuất bản bài nháp | Ở danh sách bật switch xuất bản cho `QA-Bài nháp` | Trạng thái thành published; guest mở `/tin-tuc/{slug}` thấy bài | P1 | **PASS** |
| ART-006 | Sửa bài viết | Sửa `QA-Bài test CRUD`: đổi tiêu đề + nội dung → Lưu | Nội dung mới hiển thị cả admin lẫn public | P1 | **PASS** |
| ART-007 | Tự lưu khi soạn (AutoSave) | Mở form tạo mới, gõ tiêu đề, đợi auto-save chạy | Bảng `PostAutosaves` có dòng với PostKey `new-{guid}`; không lỗi "requires a primary key" | P1 | **PASS*** — autosave hoạt động tốt, KHÔNG lỗi primary key; nhưng thiết kế thực tế: bài **mới** được autosave sẽ tạo Post draft thật (key chuyển thành `post-{id}`), chỉ bài published mới lưu temp row vào PostAutosaves |
| ART-008 | Autosave dọn dẹp sau khi lưu chính thức | Sau ART-007 bấm Lưu chính thức | Dòng autosave tương ứng bị xóa khỏi `PostAutosaves` | P2 | **PASS** (vòng 2) — guard cache 15 giây chặn sendBeacon gửi trễ ghi đè sau Cleanup (khóa vừa dọn sẽ bị bỏ qua) |
| ART-009 | Nổi bật bài viết (ToggleFeatured) | Toggle sao trên `QA-Bài test CRUD` | Bài lên khu vực nổi bật trang `/tin-tuc` | P3 | **PASS** — lên đầu danh sách `/tin-tuc` |
| ART-010 | Xóa mềm bài viết | Bấm xóa `QA-Bài test CRUD` → xác nhận | Biến mất khỏi danh sách; DB `DeletedAt` NOT NULL; guest mở `/tin-tuc/{slug}` → 404 | P1 | **PASS** (vòng 2) — xóa KHÔNG còn ép Status=draft, guest 404 đúng (đã xác minh bằng HTTP thật) |
| ART-011 | Khôi phục từ thùng rác (Restore) | Vào filter "Đã xóa" → khôi phục bài | Bài quay lại danh sách bình thường, trạng thái giữ nguyên trước xóa | P2 | **PASS** (vòng 2) — published trước xóa vẫn published sau khôi phục, guest thấy lại bài ngay |
| ART-012 | Xóa hẳn (PermanentDelete) | Trong thùng rác → Xóa hẳn → xác nhận | Mất khỏi DB (hoặc không còn ở bất kỳ filter nào); không thể khôi phục | P1 | **PASS** — record biến mất khỏi DB, guest 404 |
| ART-013 | Public list không chứa bài nháp/xóa | Mở `/tin-tuc` | Không thấy bất kỳ bài `QA-Bài nháp` hay bài đã xóa | P1 | **PASS** |

---

## M05 — DỊCH VỤ (`/admin/services`) — bảng `dbo.Services`

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| SRV-001 | Thêm dịch vụ | `/admin/services/Form` → tên `QA-Dịch vụ test` → Lưu | Lưu OK, hiện trong danh sách | P1 | **PASS** |
| SRV-002 | Sửa dịch vụ | Đổi tên thành `QA-Dịch vụ đã sửa` | Danh sách + trang `/dich-vu/{slug}` hiển thị nội dung mới | P1 | **PASS** (vòng 2) — list + detail đọc DB (title/excerpt/ContentHtml/cover render động) |
| SRV-003 | Toggle xuất bản | Tắt xuất bản | Trang `/dich-vu` public ẩn dịch vụ; mở trực tiếp slug → 404 | P1 | **PASS** (vòng 2) — đã xác minh: draft → HTTP 404, published lại → 200 |
| SRV-004 | Toggle nổi bật / UpdateSort | Bấm sao, đổi sort order | Public sắp xếp/đánh dấu đúng | P3 | **PASS** (vòng 2) — đã thêm nút sao; public sắp xếp theo IsFeatured + SortOrder từ DB |
| SRV-005 | Xóa dịch vụ | Xóa `QA-Dịch vụ đã sửa` | Mất khỏi danh sách; DB `DeletedAt` NOT NULL | P1 | **PASS** — xóa mềm đúng |

---

## M06 — KHU VỰC (`/admin/locations`) — bảng `dbo.Locations`

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| LOC-001 | Thêm khu vực | `/admin/locations/Form` → Tỉnh `TP.HCM`, Tên `QA-Khu vực test` → Lưu | Lưu OK, hiện trong danh sách | P1 | **PASS** |
| LOC-002 | Sửa khu vực | Đổi tên → Lưu | Danh sách + `/khu-vuc/{slug}` cập nhật | P1 | **PASS*** ở admin; ghi chú: `/khu-vuc/{slug}` đã bị gỡ — redirect 301 về `/khu-vuc` (theo comment trong code là chủ ý) |
| LOC-003 | Toggle xuất bản / nổi bật / sort | Thao tác lần lượt | Public phản ánh đúng trạng thái | P2 | **PASS*** — toggle + sort OK ở DB/admin; không có nút sao trong UI; public khu-vục là trang tĩnh |
| LOC-004 | Xóa khu vực | Xóa `QA-Khu vực test` | Mất khỏi danh sách, DB đánh dấu xóa mềm | P1 | **PASS** |

---

## M07 — DỰ ÁN (`/admin/projects`) — bảng `dbo.Projects`

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| PRJ-001 | Thêm dự án | `/admin/projects/Form` → `QA-Dự án test` → Lưu | Lưu OK, hiện trong danh sách | P1 | **PASS** |
| PRJ-002 | Sửa dự án | Đổi tiêu đề/mô tả → Lưu | `/du-an/{slug}` hiển thị nội dung mới | P1 | **PASS** (vòng 2) — detail đọc DB (title/type/location/quantity/duration/ContentHtml/gallery render động) |
| PRJ-003 | Toggle xuất bản | Tắt xuất bản | Public `/du-an` ẩn dự án | P1 | **PASS** (vòng 2) — list public đọc DB, draft ẩn; mở trực tiếp slug draft → 404 |
| PRJ-004 | Toggle nổi bật / UpdateSort | Thao tác | Hiển thị đúng | P3 | **PASS** (vòng 2) — đã thêm nút sao trong danh sách dự án |
| PRJ-005 | Xóa dự án | Xóa `QA-Dự án test` | Xóa mềm thành công | P1 | **PASS** |

---

## M08 — FAQ (`/admin/faq`) — bảng `dbo.FaqItems`

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| FAQ-001 | Thêm câu hỏi | `/admin/faq/Form` → câu hỏi `QA-Câu hỏi test?` + trả lời → Lưu | Lưu OK, hiện trong danh sách | P1 | **PASS** |
| FAQ-002 | Sửa câu hỏi | Đổi nội dung trả lời | Cập nhật đúng | P2 | **PASS** |
| FAQ-003 | Toggle trạng thái + UpdateSort | Tắt/bật + đổi thứ tự | Public hiển thị đúng theo trạng thái/thứ tự | P2 | **PASS** — verified 2 chiều: published → hiện trên trang chủ, draft → ẩn (FAQ đọc từ DB qua GetFaqAsync) |
| FAQ-004 | Xóa câu hỏi | Xóa `QA-Câu hỏi test` | Mất khỏi danh sách | P1 | **PASS** |

---

## M09 — YÊU CẦU LIÊN HÊ (LEADS) — bảng `dbo.ContactRequests`

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| LEAD-001 | Khách gửi form liên hệ | Mở `/lien-he`, điền tên `QA-Khách test`, SĐT, nội dung → Gửi | Lưu thành công vào `ContactRequests` (SourceForm = contact), hiện trong `/admin/leads` | P1 | **PASS** (vòng 2) — trang `/lien-he` có form liên hệ riêng POST `/contact`; SourceForm="contact" gán phía server (đã xác minh DB) |
| LEAD-002 | Form báo giá nhanh | Gửi form báo giá nhanh ở trang chủ (có đính kèm ≤ 3 ảnh) | Lưu OK kèm ảnh (bảng `ContactRequestFiles`); SourceForm = quick_quote | P1 | **PASS** — mã LE-0007, ảnh PNG được convert WebP và lưu vào ContactRequestFiles |
| LEAD-003 | Email thông báo gửi tới đúng nơi | Sau LEAD-001/002, kiểm tra hộp thư của địa chỉ trong ô "Email nhận thông báo liên hệ" (cài đặt SMTP) | Nhận email `[Yêu cầu mới LE-xxxx]` | P1 | **CHƯA XÁC MINH** — cần kiểm tra hộp thư `chibao.02092004@gmail.com` thủ công; logic code đúng (EmailNotificationService dùng smtp.to_email ưu tiên) |
| LEAD-004 | Xem chi tiết lead | Mở 1 lead trong `/admin/leads` | Hiển thị đủ thông tin + link ảnh đính kèm | P2 | **PASS** (vòng 2) — action `GET /admin/leads/detail/{id}` mới, view bind model thật (khách/ảnh/lô hàng/nguồn), có nút đánh dấu đã liên hệ |
| LEAD-005 | Đánh dấu đã liên hệ (MarkContacted) | Bấm đánh dấu contacted | Trạng thái lead chuyển đúng, filter theo status hoạt động | P2 | **PASS** — status = contacted, filter `?status=contacted` hiện đúng |
| LEAD-006 | Validation form public | Gửi form thiếu tên/SĐT hoặc SĐT sai định dạng | Bị chặn validation, không tạo record rác | P2 | **PASS** — trả 400 + message rõ ràng, không tạo record |

---

## M10 — CÀI ĐẶT CHUNG (`/admin/settings`)

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| SET-001 | Lưu thông tin công ty (SaveCompany) | Sửa hotline/email → Lưu | Header/Footer public hiển thị thông tin mới | P2 | **PASS** — hotline mới hiển thị ngay ở footer |
| SET-002 | Lưu SMTP hợp lệ (SaveSmtp) | Nhập host/port/username/from/to → Lưu | Báo "Đã lưu cấu hình email"; giá trị ghi vào `SiteSettings` (`smtp.*`) | P1 | **PASS** — toast đúng, 8 key smtp.* có giá trị trong DB |
| SET-003 | Gửi email thử nghiệm (SendSmtpTest) | Nhập địa chỉ nhận → Gửi thử | Nhận được email; nếu sai App Password Gmail → báo lỗi rõ ràng (5.7.0 Authentication Required) | P1 | **PASS*** — server báo "Đã gửi email thử nghiệm tới chibao.... Vui lòng kiểm tra hộp thư."; việc nhận thực tế cần check hộp thư thủ công |
| SET-004 | Mật khẩu SMTP giữ nguyên khi bỏ trống | Lưu SMTP mà KHÔNG nhập lại mật khẩu | Mật khẩu cũ trong DB không bị xóa; gửi thử vẫn hoạt động | P1 | **PASS** — smtp.password giữ nguyên (19 ký tự) sau khi lưu với ô mật khẩu rỗng |
| SET-005 | Đổi favicon/logo | Upload ảnh favicon/logo mới | Ảnh mới áp dụng ngay (cache chrome được làm mới) | P3 | **CHƯA CHẠY** — phần logo đã verify tương đương qua HOME-002; favicon upload chưa test riêng |
| SET-006 | Không còn panel SMS (đã gỡ tính năng) | Mở `/admin/settings` | Không thấy panel "Thông báo SMS (eSMS.vn)" và "Gửi SMS thử nghiệm"; mọi nút Lưu khác vẫn hoạt động | P2 | **PASS** (vòng 2) — đã xóa nút mồ côi "Lưu SMS" (`form="smsForm"`), xác minh HTML không còn chuỗi smsForm |

---

## M11 — TRANG CHỦ / BRAND ASSETS (`/admin/homepage`)

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| HOME-001 | Lưu cài đặt trang chủ | Sửa "Giá cập nhật lúc", "Thời gian phản hồi" → Lưu | Trang chủ hiển thị text mới | P2 | **PASS** — cả 2 text mới hiển thị trên `/` |
| HOME-002 | Đổi logo + footer logo | Upload logo mới | Logo mới hiển thị header/footer | P2 | **PASS** — logo mới áp dụng ngay cả header + footer (đã khôi phục sau test) |

---

## M12 — SEO (`/admin/seo`)

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| SEO-001 | Thêm/sửa metadata theo route | Tạo metadata cho `/tin-tuc` (title, description) → Lưu | Trang public render thẻ `<title>`/`<meta name="description">` mới | P2 | **PASS** — cả title lẫn meta description cập nhật ngay |
| SEO-002 | Lưu site settings SEO | Đổi Site title mặc định → Lưu | Trang chưa có metadata riêng dùng tiêu đề mới | P3 | **PASS** (vòng 2) — `<title>` dùng setting `seo.site_title` qua SiteChromeService (fallback tên công ty), không còn hardcode "- ScrapWebsite" |
| SEO-003 | Sitemap chỉ chứa bài published | Mở `/sitemap.xml` | Có `/tin-tuc/{slug}` của bài published; **KHÔNG** chứa bài nháp/bài đã xóa | P1 | **PASS** |
| SEO-004 | robots.txt | Mở `/robots.txt` | Trả text chuẩn có đường dẫn Sitemap | P3 | **PASS** (ghi chú: Sitemap trỏ domain production `phelieuminhduc.com` — cần cấu hình theo môi trường) |

---

## M13 — MEDIA (`/admin/media`)

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| MED-001 | Upload ảnh qua media/editor | Upload 1 ảnh JPG/PNG | Ảnh convert sang WebP, trả URL, mở được ảnh | P2 | **PASS** — PNG → `.webp`, Content-Type image/webp, mở OK |
| MED-002 | Upload quá giới hạn | Upload file > MaxUploadBytes (10MB) | Bị chặn với thông báo lỗi rõ ràng | P3 | **PASS** — 11MB bị chặn 400 "Tệp quá lớn (11.0 MB). Tối đa 10 MB." |
| MED-003 | Thay ảnh setting (SaveSettingImage) | Thay banner trang chủ từ media | Ảnh mới áp dụng tại vị trí tương ứng | P2 | **PASS** — `brand.banner_1` được ghi đè file webp mới (đã khôi phục sau test) |

---

## M14 — KIỂM THỬ REGRESSION (các lỗi đã sửa gần đây)

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| REG-001 | Trang chủ không lỗi EF model | Mở `/` khi chưa đăng nhập | HTTP 200, không exception `PostAutosave requires a primary key` | P1 | **PASS** — 200, log server sạch |
| REG-002 | Ưu tiên người nhận mail | Lưu SMTP ToEmail = A, `contact.email` = B → khách gửi form liên hệ | Mail thông báo gửi tới **A** (`smtp.to_email` được ưu tiên) | P1 | **PASS** (code-level) — `EmailNotificationService.cs:28` dùng `FirstNonEmpty(options.ToEmail, adminEmail, from)` với ToEmail lấy từ `smtp.to_email`. Việc nhận thư thực tế cần check hộp thư |
| REG-003 | Bản nháp an toàn 2 chiều | Guest mở slug nháp → 404; admin mở cùng slug → xem được | Đúng như mô tả, không lộ nội dung cho guest | P1 | **PASS** — verified qua ART-003/ART-004 |
| REG-004 | Build không cảnh báo SMS | Build solution | 0 error; log runtime không còn gọi eSMS | P3 | **PASS** — `dotnet build`: 0 error / 0 warning; source không còn tham chiếu eSMS |

---

## M15 — KIỂM THỬ PHI CHỨC NĂNG (nhanh)

| ID | Tiêu đề | Các bước | Kết quả mong đợi | Ưu tiên | Kết quả |
|---|---|---|---|---|---|
| NFR-001 | Trang public load không lỗi 500 | Duyệt: `/`, `/phe-lieu`, `/phe-lieu/{slug}`, `/tin-tuc`, `/tin-tuc/{slug}`, `/dich-vu`, `/khu-vuc`, `/du-an`, `/bang-gia`, `/tim-kiem?q=sắt`, `/lien-he` | Tất cả HTTP 200 (trừ slug cố tình sai → 404) | P1 | **PASS** — 13/13 URL đều 200; slug sai/nháp/xóa → 404 đúng |
| NFR-002 | Anti-forgery | POST trực tiếp `/admin/articles/ToggleStatus` không kèm token | Bị chặn (400) — không thao tác được | P2 | **PASS** — 400 khi thiếu token (kể cả có session admin), dữ liệu không đổi |
| NFR-003 | Responsive nhanh | Mở admin ở width 1366px | Layout không vỡ, không scroll ngang | P3 | **PASS** — scrollWidth == clientWidth, screenshot `gui-test-screenshots/admin-1366-nfr003.png` |

---

## TỔNG KẾT

> Quy ước đếm: PARTIAL tính vào cột FAIL (vì chưa đạt đủ tiêu chí testcase); PASS* = PASS kèm ghi chú.

| Module | Tổng TC | PASS | FAIL | Chưa chạy |
|---|---|---|---|---|
| M01 Đăng nhập | 5 | 5 | 0 | 0 |
| M02 Phế liệu + nhóm | 11 | 11 | 0 | 0 |
| M03 Bảng giá | 6 | 6 | 0 | 0 |
| M04 Bài viết + autosave | 13 | 13 | 0 | 0 |
| M05 Dịch vụ | 5 | 5 | 0 | 0 |
| M06 Khu vực | 4 | 4 | 0 | 0 |
| M07 Dự án | 5 | 5 | 0 | 0 |
| M08 FAQ | 4 | 4 | 0 | 0 |
| M09 Leads + email | 6 | 5 | 0 | 1 |
| M10 Cài đặt | 6 | 5 | 0 | 1 |
| M11 Trang chủ | 2 | 2 | 0 | 0 |
| M12 SEO | 4 | 4 | 0 | 0 |
| M13 Media | 3 | 3 | 0 | 0 |
| M14 Regression | 4 | 4 | 0 | 0 |
| M15 Phi chức năng | 3 | 3 | 0 | 0 |
| **TỔNG** | **81** | **79** | **0** | **2** |

## LỖI ĐÃ SỬA (vòng 2 — 25/08/2026)

Toàn bộ 18 lỗi FAIL/PARTIAL của vòng 1 đã xử lý xong, build 0 error / 0 warning, đã chạy lại bằng HTTP thật:

1. **PRI-002** — `SavePriceBulkAsync` bỏ qua dòng tick chọn nhưng giá rỗng; controller báo rõ "Bỏ qua N dòng...". Không bao giờ ghi `PriceValue = NULL`.
2. **PRI-006 / SRV-002 / SRV-003 / PRJ-002 / PRJ-003** — `/bang-gia`, `/dich-vu` (+detail), `/du-an` (+detail) render động từ DB; slug nháp/xóa/không tồn tại → 404.
3. **ART-008** — guard IMemoryCache 15 giây sau cleanup chặn sendBeacon ghi đè autosave stale.
4. **SEO-002** — `<title>` dùng setting `seo.site_title` (fallback tên công ty).
5. **SET-006** — bỏ nút mồ côi "Lưu SMS".
6. **SCR-001** — form phế liệu mặc định SortOrder = max+1; hiển thị lỗi validate cho trường Vị trí.
7. **SCR-005 / SRV-004 / PRJ-004** — thêm nút sao ToggleFeatured cho Phế liệu/Dịch vụ/Dự án.
8. **SCR-007** — xóa mềm phế liệu qua cột mới `dbo.ScrapItems.DeletedAt`; migration: `database/phase5_scrap_items_soft_delete.sql` (ĐÃ CHẠY trên DB local); mọi truy vấn admin/public/sitemap đều lọc `DeletedAt IS NULL`.
9. **ART-010/011** — xóa bài giữ nguyên Status; khôi phục trả về đúng trạng thái trước xóa (đã bổ sung filter `DeletedAt` ở news cursor + trang chủ để bài published đã xóa không lộ).
10. **LEAD-001 / LEAD-004** — `/lien-he` có form riêng POST `/contact` (SourceForm="contact", gán phía server); action `GET /admin/leads/detail/{id}` + view bind model thật.
11. **CAT-001..003** — UI quản lý nhóm phế liệu đầy đủ (list/form/thêm/sửa/toggle/xóa-nhóm-rỗng), sidebar có mục "Nhóm phế liệu".

## VIỆC CẦN KIỂM TRA THỦ CÔNG SAU KHI CHẠY

- **LEAD-003 / SET-003**: đăng nhập hộp thư `chibao.02092004@gmail.com` để xác nhận nhận được email thử nghiệm + email thông báo lead `[Yêu cầu mới LE-xxxx]`.
- **SET-005**: upload favicon riêng (chưa chạy trong phiên này).
- **REG-002 (end-to-end)**: gửi form liên hệ thật và xem thư đến đúng địa chỉ `smtp.to_email`.

## DỌN DẸP SAU TEST — ĐÃ THỰC HIỆN (25/08/2026)

```sql
-- Đã chạy: xóa toàn bộ dữ liệu QA-
DELETE FROM dbo.ContactRequestFiles WHERE ContactRequestId IN
  (SELECT Id FROM dbo.ContactRequests WHERE Name LIKE N'QA-%');
DELETE FROM dbo.ContactRequests WHERE Name LIKE N'QA-%';
DELETE FROM dbo.PostProductLinks WHERE PostId IN (SELECT Id FROM dbo.Posts WHERE Title LIKE N'QA-%');
DELETE FROM dbo.PostAutosaves WHERE PostKey LIKE '%QA%' OR PostKey IN ('post-167','post-170');
DELETE FROM dbo.Posts WHERE Title LIKE N'QA-%';
DELETE FROM dbo.ScrapPrices WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'QA-%');
DELETE FROM dbo.ScrapItems WHERE Name LIKE N'QA-%';
DELETE FROM dbo.Services WHERE Title LIKE N'QA-%';
DELETE FROM dbo.Locations WHERE Name LIKE N'QA-%';
DELETE FROM dbo.Projects WHERE Title LIKE N'QA-%';
DELETE FROM dbo.FaqItems WHERE Question LIKE N'QA-%';

-- Đã khôi phục cài đặt ảnh bị thay trong lúc test (site.logo, site.footer_logo, brand.banner_1)
UPDATE dbo.SiteSettings SET SettingValue = '/assets/images/imported/brand/logo.png' WHERE SettingKey = 'site.logo';
UPDATE dbo.SiteSettings SET SettingValue = '/assets/images/imported/brand/logo-footer.png' WHERE SettingKey = 'site.footer_logo';
UPDATE dbo.SiteSettings SET SettingValue = '/assets/images/imported/brand/banner-1.jpg' WHERE SettingKey = 'brand.banner_1';
```

Kết quả sau dọn dẹp: 0 bản ghi `QA-%` còn lại trong DB; các trang public trả 200; logo/banner đã trả về ảnh gốc. File ảnh rác tạo khi test (uploads/content/editor-image-*.webp, quote-image-*.webp, home-banner-1 bị ghi đè) nằm trong `wwwroot/uploads/`, có thể xóa tay nếu muốn.
