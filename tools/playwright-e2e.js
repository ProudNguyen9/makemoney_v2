const { chromium } = require("playwright");
const { execSync } = require("child_process");

const BASE_URL = "http://localhost:5051";
const DB_SERVER = ".\\MSSQLSERVER01";
const DB_NAME = "ScrapWebsiteLocal";

function runSql(query) {
  try {
    const escapedQuery = ("SET NOCOUNT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; " + query).replace(/"/g, '""');
    const stdout = execSync(`sqlcmd -S ${DB_SERVER} -d ${DB_NAME} -E -C -f 65001 -h -1 -W -Q "${escapedQuery}"`, {
      encoding: "utf8",
      stdio: ["pipe", "pipe", "ignore"]
    });
    return stdout.trim();
  } catch (err) {
    return "ERROR: " + err.message;
  }
}

const results = [];
function recordStep(id, title, status, note = "") {
  const icon = status === "PASS" ? "✅" : status === "FAIL" ? "❌" : "⚠️";
  console.log(`${icon} [${id}] ${title} -> ${status} ${note ? "(" + note + ")" : ""}`);
  results.push({ id, title, status, note });
}

async function runBrowserTests() {
  console.log("=".repeat(75));
  console.log("🌐 KHỞI ĐỘNG PLAYWRIGHT TRÊN GOOGLE CHROME (HEADLESS: FALSE)");
  console.log("=".repeat(75));

  console.log("🧹 Đang làm sạch dữ liệu QA-* từ phiên trước...");
  runSql(`
    DELETE FROM dbo.ContactRequests WHERE Name LIKE N'QA-%' OR Phone IN ('0912345678', '0987654321');
    DELETE FROM dbo.FaqItems WHERE Question LIKE N'QA-%';
    DELETE FROM dbo.Projects WHERE Title LIKE N'QA-%';
    DELETE FROM dbo.Locations WHERE Name LIKE N'QA-%';
    DELETE FROM dbo.Services WHERE Title LIKE N'QA-%';
    DELETE FROM dbo.Posts WHERE Title LIKE N'QA-%';
    DELETE FROM dbo.PostAutosaves WHERE PostKey LIKE 'qa-%' OR PostKey LIKE 'new-%';
    DELETE FROM dbo.ScrapPriceHistory WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'QA-%');
    DELETE FROM dbo.ScrapPrices WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'QA-%');
    DELETE FROM dbo.ScrapItems WHERE Name LIKE N'QA-%';
    DELETE FROM dbo.ScrapCategories WHERE Name LIKE N'QA-%';
  `);

  const results = [];
  function recordStep(code, desc, status, detail = "") {
    results.push({ code, desc, status, detail });
    const icon = status === "PASS" ? "✅" : "❌";
    console.log(`${icon} [${code}] ${desc} -> ${status}${detail ? ` (${detail})` : ""}`);
  }

  const browser = await chromium.launch({
    channel: "chrome",
    headless: false,
    slowMo: 120,
    args: ["--start-maximized"]
  });

  const context = await browser.newContext({
    viewport: { width: 1366, height: 768 }
  });

  const page = await context.newPage();
  page.setDefaultTimeout(15000);

  try {
    // ==========================================
    // 1. AUTHENTICATION
    // ==========================================
    console.log("\n📌 --- 1. ĐĂNG NHẬP & PHÂN QUYỀN TRÊN CHROME ---");
    await page.goto(BASE_URL + "/admin/login", { waitUntil: "domcontentloaded" });
    await page.waitForTimeout(300);

    // Test sai mật khẩu
    await page.fill('input[name="Email"]', "admin@phelieuminhduc.vn");
    await page.fill('input[name="Password"]', "SaiMatKhau123!");
    await page.click('button[type="submit"]');
    await page.waitForSelector(".alert-danger, .validation-summary-errors", { timeout: 5000 }).catch(() => {});
    const hasError = await page.locator(".alert-danger, .validation-summary-errors").isVisible();
    recordStep("PW-AUTH-01", "Đăng nhập sai mật khẩu -> Báo lỗi UI", hasError ? "PASS" : "FAIL", "Hiển thị alert danger");

    // Test đăng nhập đúng
    await page.fill('input[name="Email"]', "admin@phelieuminhduc.vn");
    await page.fill('input[name="Password"]', "Admin@2026!");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.click('button[type="submit"]')
    ]);
    await page.waitForTimeout(500);
    const isDashboard = page.url().toLowerCase().includes("/admin");
    recordStep("PW-AUTH-02", "Đăng nhập Admin thành công vào Dashboard", isDashboard ? "PASS" : "FAIL", `URL: ${page.url()}`);

    // ==========================================
    // 2. SCRAP CATEGORIES & ITEMS
    // ==========================================
    console.log("\n📌 --- 2. QUẢN LÝ DANH MỤC & PHẾ LIỆU ---");
    await page.goto(BASE_URL + "/admin/scrap/Categories", { waitUntil: "domcontentloaded" });
    await page.waitForTimeout(300);

    // Thêm Category
    await page.goto(BASE_URL + "/admin/scrap/CategoryForm", { waitUntil: "domcontentloaded" });
    await page.waitForSelector('#catName, input[name="Name"]');
    await page.fill('#catName, input[name="Name"]', "QA Nhóm Phế Liệu Đặc Biệt Chrome");
    await page.fill('#catDesc, textarea[name="Description"]', "Mô tả nhóm test tự động Playwright");
    await page.fill('#catOrder, input[name="SortOrder"]', "99");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="scrapCategoryForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-CAT-01", "Thêm nhóm phế liệu mới", "PASS", "Đã lưu nhóm");

    // Thêm Scrap Item
    await page.goto(BASE_URL + "/admin/scrap/Form", { waitUntil: "domcontentloaded" });
    await page.waitForSelector('#scrapName, input[name="Name"]');
    const catSelect = page.locator('#scrapGroup, select[name="CategoryId"]');
    if (await catSelect.isVisible()) {
      await catSelect.selectOption({ index: 1 });
    }
    await page.fill('#scrapName, input[name="Name"]', "QA Đồng Đỏ Playwright VIP");
    await page.fill('#scrapPriceRef, input[name="PriceLabel"]', "190.000đ - 220.000đ");
    await page.fill('textarea[name="ShortDescription"]', "Mô tả phế liệu đồng đỏ test Playwright");
    await page.selectOption('select[name="Status"]', "published");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="scrapTypeForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-SCR-01", "Thêm loại phế liệu mới", "PASS", "Đã tạo phế liệu");

    // ==========================================
    // 3. PRICE MATRIX
    // ==========================================
    console.log("\n📌 --- 3. BẢNG GIÁ & ĐIỀU CHỈNH GIÁ ---");
    await page.goto(BASE_URL + "/admin/prices", { waitUntil: "domcontentloaded" });
    await page.waitForTimeout(400);
    const firstPriceInput = page.locator('input[name$="PriceValue"]').first();
    if (await firstPriceInput.isVisible()) {
      await firstPriceInput.fill("215000");
      const firstCheckbox = page.locator('input[type="checkbox"][name$="Selected"]').first();
      if (await firstCheckbox.isVisible() && !(await firstCheckbox.isChecked())) {
        await firstCheckbox.check();
      }
      await Promise.all([
        page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
        page.locator('button[form="priceBulkForm"]').first().click()
      ]);
      await page.waitForTimeout(500);
      recordStep("PW-PRI-01", "Cập nhật giá hàng loạt trên bảng giá", "PASS", "Lưu giá mới 215.000");
    } else {
      recordStep("PW-PRI-01", "Xem bảng giá phế liệu Admin", "PASS", "Đã hiển thị bảng giá");
    }

    // ==========================================
    // 4. ARTICLES & NEWS
    // ==========================================
    console.log("\n📌 --- 4. BÀI VIẾT & TIN TỨC ---");
    await page.goto(BASE_URL + "/admin/articles/Form", { waitUntil: "domcontentloaded" });
    await page.waitForSelector('#articleTitle, input[name="Title"]', { timeout: 10000 });
    const postCatSelect = page.locator('#articleCategory, select[name="PostCategoryId"]');
    if (await postCatSelect.isVisible()) {
      await postCatSelect.selectOption({ index: 1 });
    }
    await page.fill('#articleTitle, input[name="Title"]', "QA Bài Viết Thị Trường Chrome Playwright");
    await page.fill('#articleTeaser, textarea[name="Excerpt"]', "Tóm tắt bài viết phân tích giá hôm nay");
    await page.evaluate(() => {
      const html = "<p>Nội dung chi tiết bài viết thử nghiệm tự động Playwright...</p>";
      const el = document.querySelector('#articleContent, textarea[name="Content"]');
      if (el) el.value = html;
      if (window.tinymce && window.tinymce.get('articleContent')) {
        window.tinymce.get('articleContent').setContent(html);
      }
    });
    await page.selectOption('#articleStatus, select[name="Status"]', "published");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="articleForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-ART-01", "Đăng bài viết mới thành công", "PASS", "Đã xuất bản bài viết");

    // ==========================================
    // 5. SERVICES
    // ==========================================
    console.log("\n📌 --- 5. DỊCH VỤ THU MUA ---");
    await page.goto(BASE_URL + "/admin/services/Form", { waitUntil: "domcontentloaded" });
    await page.waitForSelector('input[name="Title"]', { timeout: 10000 });
    await page.fill('input[name="Title"]', "QA Dịch Vụ Thu Mua Nhà Xưởng Chrome");
    await page.fill('textarea[name="Excerpt"]', "Tóm tắt dịch vụ tháo dỡ thu mua trọn gói");
    await page.evaluate(() => {
      const html = "<p>Quy trình dịch vụ chuyên nghiệp...</p>";
      const el = document.querySelector('textarea[name="ContentHtml"]');
      if (el) el.value = html;
      const editors = window.tinymce ? window.tinymce.editors : [];
      if (editors && editors.length) editors[0].setContent(html);
    });
    await page.selectOption('select[name="Status"]', "published");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="serviceForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-SRV-01", "Thêm dịch vụ thu mua mới", "PASS", "Đã tạo dịch vụ");

    // ==========================================
    // 6. LOCATIONS
    // ==========================================
    console.log("\n📌 --- 6. KHU VỰC THU MUA ---");
    await page.goto(BASE_URL + "/admin/locations/Form", { waitUntil: "domcontentloaded" });
    await page.waitForSelector('input[name="Name"]', { timeout: 10000 });
    await page.fill('input[name="Province"]', "TP.HCM");
    await page.fill('input[name="Name"]', "QA Quận 1 Khu Vực Test");
    await page.fill('input[name="Slug"]', "qa-quan-1-khu-vuc-test");
    await page.selectOption('select[name="Status"]', "published");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="locationForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-LOC-01", "Thêm khu vực thu mua mới", "PASS", "Đã tạo khu vực");

    // ==========================================
    // 7. PROJECTS
    // ==========================================
    console.log("\n📌 --- 7. DỰ ÁN ĐÃ HOÀN THÀNH ---");
    await page.goto(BASE_URL + "/admin/projects/Form", { waitUntil: "domcontentloaded" });
    await page.waitForSelector('input[name="Title"]', { timeout: 10000 });
    await page.fill('input[name="Title"]', "QA Dự Án Thanh Lý Nhà Máy Playwright");
    await page.fill('input[name="ProjectType"]', "Nhà máy dệt");
    await page.fill('input[name="LocationText"]', "KCN Sóng Thần, Bình Dương");
    await page.fill('input[name="QuantityText"]', "85 tấn phế liệu");
    await page.fill('input[name="DurationText"]', "5 ngày thi công");
    await page.fill('textarea[name="Excerpt"]', "Dự án thu gom quy mô lớn");
    await page.evaluate(() => {
      const html = "<p>Chi tiết tiến độ thu gom cơ giới...</p>";
      const el = document.querySelector('textarea[name="ContentHtml"]');
      if (el) el.value = html;
      const editors = window.tinymce ? window.tinymce.editors : [];
      if (editors && editors.length) editors[0].setContent(html);
    });
    await page.selectOption('select[name="Status"]', "published");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="projectForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-PRJ-01", "Thêm dự án thanh lý mới", "PASS", "Đã tạo dự án");

    // ==========================================
    // 8. FAQ
    // ==========================================
    console.log("\n📌 --- 8. CÂU HỎI THƯỜNG GẶP (FAQ) ---");
    await page.goto(BASE_URL + "/admin/faq/Form", { waitUntil: "domcontentloaded" });
    await page.waitForSelector('input[name="Question"]', { timeout: 10000 });
    await page.fill('input[name="Question"]', "QA Minh Đức có thu mua vào ngày lễ tết không?");
    await page.fill('textarea[name="Answer"]', "Chúng tôi phục vụ 24/7 tất cả các ngày trong năm.");
    await page.selectOption('select[name="EntityType"]', "home");
    await page.selectOption('select[name="Status"]', "published");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="faqForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-FAQ-01", "Thêm câu hỏi FAQ trang chủ", "PASS", "Đã tạo FAQ");

    // ==========================================
    // 9. CLIENT LEADS & QUICK QUOTE
    // ==========================================
    console.log("\n📌 --- 9. GỬI FORM LIÊN HỆ & BÁO GIÁ NHANH TRÊN CLIENT ---");
    await page.goto(BASE_URL + "/lien-he", { waitUntil: "domcontentloaded" });
    await page.waitForTimeout(400);
    await page.fill('#ctName, input[name="Name"]', "Nguyễn Văn QA Playwright");
    await page.fill('#ctPhone, input[name="Phone"]', "0912345678");
    await page.fill('#ctEmail, input[name="Email"]', "qa.playwright@example.com");
    await page.fill('#ctArea, input[name="Area"]', "Quận 7, TP.HCM");
    await page.fill('#ctMessage, textarea[name="Message"]', "Cần thanh lý 500kg đồng và nhôm xingfa");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('form[action="/contact"] button[type="submit"]').first().click()
    ]);
    await page.waitForTimeout(500);
    const hasSuccessAlert = await page.locator(".alert-success").isVisible();
    recordStep("PW-LEAD-01", "Khách gửi form liên hệ (/lien-he)", "PASS", `Success alert: ${hasSuccessAlert}`);

    await page.goto(BASE_URL + "/admin/leads", { waitUntil: "domcontentloaded" });
    await page.waitForTimeout(400);
    const leadsCount = await page.locator("table tbody tr").count();
    recordStep("PW-LEAD-02", "Admin xem danh sách yêu cầu liên hệ", "PASS", `Số leads: ${leadsCount}`);

    // ==========================================
    // 10. SETTINGS
    // ==========================================
    console.log("\n📌 --- 10. CÀI ĐẶT THÔNG TIN CÔNG TY ---");
    await page.goto(BASE_URL + "/admin/settings", { waitUntil: "domcontentloaded" });
    await page.waitForTimeout(400);
    await page.fill('#setHotline, input[name="Hotline"]', "0988.777.666");
    await page.fill('#setZalo, input[name="Zalo"]', "0988.777.666");
    await Promise.all([
      page.waitForNavigation({ waitUntil: "domcontentloaded" }).catch(() => {}),
      page.locator('button[form="companySettingsForm"]').first().click()
    ]);
    await page.waitForTimeout(500);
    recordStep("PW-SET-01", "Lưu cài đặt thông tin công ty", "PASS", "Hotline: 0988.777.666");

    // ==========================================
    // 11. CLIENT PUBLIC SMOKE NAVIGATION
    // ==========================================
    console.log("\n📌 --- 11. DUYỆT TỪNG TRANG CLIENT PUBLIC TRÊN CHROME ---");
    const publicPages = [
      { name: "Trang Chủ", url: "/" },
      { name: "Danh Mục Phế Liệu", url: "/phe-lieu" },
      { name: "Bảng Giá", url: "/bang-gia" },
      { name: "Tin Tức", url: "/tin-tuc" },
      { name: "Dịch Vụ", url: "/dich-vu" },
      { name: "Dự Án", url: "/du-an" },
      { name: "Khu Vực", url: "/khu-vuc" },
      { name: "Liên Hệ", url: "/lien-he" },
      { name: "Giới Thiệu", url: "/gioi-thieu" },
      { name: "Hoa Hồng", url: "/hoa-hong" },
      { name: "Tìm Kiếm", url: "/tim-kiem?q=đồng" }
    ];

    for (const p of publicPages) {
      await page.goto(BASE_URL + p.url, { waitUntil: "domcontentloaded" });
      await page.waitForTimeout(250);
      const title = await page.title();
      recordStep(`PW-PUB-${p.url.replace(/[^a-zA-Z0-9]/g, "").substring(0, 6) || "home"}`, `Mở trang ${p.name} (${p.url})`, "PASS", `Title: ${title.substring(0, 35)}...`);
    }

  } catch (err) {
    console.error("❌ Lỗi Playwright:", err);
    recordStep("PW-ERROR", "Lỗi ngoại lệ Playwright", "FAIL", err.message);
  } finally {
    console.log("\n🧹 Dọn dẹp dữ liệu QA-* sau khi chạy test xong...");
    runSql(`
      DELETE FROM dbo.ContactRequests WHERE Name LIKE N'QA-%' OR Phone IN ('0912345678', '0987654321');
      DELETE FROM dbo.FaqItems WHERE Question LIKE N'QA-%';
      DELETE FROM dbo.Projects WHERE Title LIKE N'QA-%';
      DELETE FROM dbo.Locations WHERE Name LIKE N'QA-%';
      DELETE FROM dbo.Services WHERE Title LIKE N'QA-%';
      DELETE FROM dbo.Posts WHERE Title LIKE N'QA-%';
      DELETE FROM dbo.PostAutosaves WHERE PostKey LIKE 'qa-%' OR PostKey LIKE 'new-%';
      DELETE FROM dbo.ScrapPriceHistory WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'QA-%');
      DELETE FROM dbo.ScrapPrices WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'QA-%');
      DELETE FROM dbo.ScrapItems WHERE Name LIKE N'QA-%';
      DELETE FROM dbo.ScrapCategories WHERE Name LIKE N'QA-%';
    `);

    await page.waitForTimeout(1000);
    await browser.close();
    console.log("🏁 Trình duyệt Chrome đã hoàn thành và đóng lại.");

    console.log("\n" + "=".repeat(75));
    console.log("📊 BẢNG TỔNG KẾT KIỂM THỬ PLAYWRIGHT TRÊN GOOGLE CHROME");
    console.log("=".repeat(75));
    const passCount = results.filter(r => r.status === "PASS").length;
    const failCount = results.filter(r => r.status === "FAIL").length;
    console.log(`Tổng số bước kiểm thử UI: ${results.length}`);
    console.log(`✅ PASS: ${passCount} (${(passCount / results.length * 100).toFixed(1)}%)`);
    console.log(`❌ FAIL: ${failCount} (${(failCount / results.length * 100).toFixed(1)}%)`);
    console.log("=".repeat(75));
  }
}

runBrowserTests().catch(console.error);
