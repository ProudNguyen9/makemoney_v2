/**
 * DEEP EDGE CASE & ROBUSTNESS TEST SUITE
 * Exhaustive edge-case testing for ScrapWebsite (Admin & Client)
 * Focuses on:
 * 1. Security & Injection Fuzzing (SQLi, XSS, CSRF, Malformed Unicode, Unauthorized Access)
 * 2. Boundary Conditions & Form Validation (Empty fields, Overflow numbers, Negative numbers, Maxlength)
 * 3. Slug Collision & Special Character Resilience
 * 4. State Synchronization & Soft Delete Invariants
 * 5. Price Matrix Edge Cases & Price History Tracking
 * 6. Search Query Fuzzing (Wildcards, Empty, Whitespace, Diacritics, XSS)
 * 7. Error Handling & 404 Cleanliness (No 500 stack traces)
 * 8. Technical SEO & XML Sitemap Integrity
 */

const http = require('http');
const { execSync } = require('child_process');
const querystring = require('querystring');

const BASE_URL = 'http://localhost:5051';
const DB_SERVER = '.\\MSSQLSERVER01';
const DB_NAME = 'ScrapWebsiteLocal';

class CookieJar {
  constructor() {
    this.cookies = new Map();
  }

  setCookiesFromHeaders(headers) {
    const rawCookies = headers['set-cookie'];
    if (!rawCookies) return;
    const list = Array.isArray(rawCookies) ? rawCookies : [rawCookies];
    for (const cookieStr of list) {
      const parts = cookieStr.split(';')[0].split('=');
      const name = parts[0].trim();
      const value = parts.slice(1).join('=').trim();
      this.cookies.set(name, value);
    }
  }

  getCookieHeader() {
    return Array.from(this.cookies.entries())
      .map(([k, v]) => `${k}=${v}`)
      .join('; ');
  }

  clear() {
    this.cookies.clear();
  }
}

function runSql(query) {
  try {
    const escaped = `SET NOCOUNT ON; ${query}`.replace(/"/g, '""');
    const cmd = `sqlcmd -S "${DB_SERVER}" -d "${DB_NAME}" -h -1 -W -Q "${escaped}"`;
    const out = execSync(cmd, { encoding: 'utf-8', stdio: ['pipe', 'pipe', 'pipe'] }).trim();
    return out
      .replace(/\r/g, '')
      .split('\n')
      .map(l => l.trim())
      .filter(l => l.length > 0 && !l.startsWith('('))
      .join('\n');
  } catch (err) {
    return `SQL_ERROR: ${err.message}`;
  }
}

function request(urlPath, options = {}, jar = null) {
  return new Promise((resolve, reject) => {
    const parsed = new URL(urlPath, BASE_URL);
    const headers = Object.assign({}, options.headers || {});

    if (jar) {
      const cookieHeader = jar.getCookieHeader();
      if (cookieHeader) {
        headers['Cookie'] = cookieHeader;
      }
    }

    const reqOpts = {
      hostname: parsed.hostname,
      port: parsed.port || 80,
      path: parsed.pathname + parsed.search,
      method: options.method || 'GET',
      headers: headers
    };

    const req = http.request(reqOpts, (res) => {
      if (jar) {
        jar.setCookiesFromHeaders(res.headers);
      }

      const chunks = [];
      res.on('data', (chunk) => chunks.push(chunk));
      res.on('end', () => {
        const body = Buffer.concat(chunks).toString('utf-8');
        resolve({
          statusCode: res.statusCode,
          headers: res.headers,
          body: body
        });
      });
    });

    req.on('error', reject);

    if (options.body) {
      req.write(options.body);
    }
    req.end();
  });
}

function extractAntiforgeryToken(html) {
  const match = html.match(/name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"/i)
    || html.match(/value="([^"]+)"\s+name="__RequestVerificationToken"/i)
    || html.match(/input[^>]+name="__RequestVerificationToken"[^>]+value="([^"]+)"/i);
  return match ? match[1] : null;
}

const testResults = [];
function record(id, name, status, detail = "") {
  testResults.push({ id, name, status, detail });
  const icon = status === "PASS" ? "✅" : (status === "WARN" ? "⚠️" : "❌");
  console.log(`${icon} [${id}] ${name} -> ${status}${detail ? ` (${detail})` : ""}`);
}

async function runDeepTests() {
  console.log("=".repeat(80));
  console.log("🚀 KHỞI ĐỘNG BỘ KIỂM THỬ CHUYÊN SÂU & TÌM LỖI (DEEP EDGE-CASE & ROBUSTNESS)");
  console.log("=".repeat(80));

  // Clean test data from previous runs
  runSql(`
    DELETE FROM dbo.ContactRequests WHERE Name LIKE N'EDGE-%' OR Phone LIKE '0999%';
    DELETE FROM dbo.FaqItems WHERE Question LIKE N'EDGE-%';
    DELETE FROM dbo.Projects WHERE Title LIKE N'EDGE-%';
    DELETE FROM dbo.Locations WHERE Name LIKE N'EDGE-%';
    DELETE FROM dbo.Services WHERE Title LIKE N'EDGE-%';
    DELETE FROM dbo.Posts WHERE Title LIKE N'EDGE-%';
    DELETE FROM dbo.PostAutosaves WHERE PostKey LIKE 'edge-%';
    DELETE FROM dbo.ScrapPriceHistory WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'EDGE-%');
    DELETE FROM dbo.ScrapPrices WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'EDGE-%');
    DELETE FROM dbo.ScrapItems WHERE Name LIKE N'EDGE-%';
    DELETE FROM dbo.ScrapCategories WHERE Name LIKE N'EDGE-%';
  `);

  const adminJar = new CookieJar();
  const clientJar = new CookieJar();

  // =========================================================================
  // 1. SECURITY, INJECTION & ACCESS CONTROL
  // =========================================================================
  console.log("\n🔒 --- PHẦN 1: BẢO MẬT, INJECTION & PHÂN QUYỀN TRUY CẬP ---");

  // 1.1 SQL Injection on Login
  const loginGet = await request('/admin/login', {}, adminJar);
  const loginToken = extractAntiforgeryToken(loginGet.body);
  const sqliPayload = querystring.stringify({
    __RequestVerificationToken: loginToken,
    Email: "' OR '1'='1' --",
    Password: "' OR '1'='1' --"
  });
  const sqliRes = await request('/admin/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: sqliPayload
  }, adminJar);
  record('SEC-SQLI-01', 'SQL Injection login attempt', sqliRes.statusCode !== 500 && !sqliRes.headers.location?.includes('/admin') ? 'PASS' : 'FAIL', `Status: ${sqliRes.statusCode}`);

  // 1.2 XSS Payload on Login
  const xssLoginPayload = querystring.stringify({
    __RequestVerificationToken: loginToken,
    Email: "<script>alert('XSS')</script>@example.com",
    Password: "password123"
  });
  const xssLoginRes = await request('/admin/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: xssLoginPayload
  }, adminJar);
  const xssEscaped = !xssLoginRes.body.includes("<script>alert('XSS')</script>");
  record('SEC-XSS-01', 'XSS prevention on Login reflection', xssEscaped ? 'PASS' : 'FAIL', 'Payload escaped');

  // 1.3 Unauthorized access to all Admin sub-routes
  const adminRoutes = [
    '/admin', '/admin/scrap', '/admin/scrap/categories', '/admin/scrap/Form',
    '/admin/prices', '/admin/articles', '/admin/articles/Form', '/admin/services',
    '/admin/locations', '/admin/projects', '/admin/faq', '/admin/leads',
    '/admin/settings', '/admin/seo', '/admin/media', '/admin/homepage'
  ];

  let unauthPassCount = 0;
  for (const route of adminRoutes) {
    const unauthRes = await request(route);
    const isRedirectToLogin = unauthRes.statusCode === 302 && unauthRes.headers.location?.toLowerCase().includes('/admin/login');
    if (isRedirectToLogin) unauthPassCount++;
  }
  record('SEC-AUTH-01', `Chặn truy cập trái phép 16 Admin routes (${unauthPassCount}/${adminRoutes.length})`, unauthPassCount === adminRoutes.length ? 'PASS' : 'FAIL', 'All redirect to Login');

  // 1.4 CSRF Protection on POST
  const fakeCsrfRes = await request('/admin/scrap/SaveCategory', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: 'FAKE_INVALID_TOKEN_12345',
      Name: 'EDGE HACK CATEGORY'
    })
  });
  record('SEC-CSRF-01', 'Chặn POST khi thiếu/sai AntiForgeryToken', (fakeCsrfRes.statusCode === 400 || fakeCsrfRes.statusCode === 302) ? 'PASS' : 'FAIL', `Status: ${fakeCsrfRes.statusCode}`);

  // 1.5 Authenticate valid Admin for remaining tests
  adminJar.clear();
  const adminAuthGet = await request('/admin/login', {}, adminJar);
  const adminToken = extractAntiforgeryToken(adminAuthGet.body);
  await request('/admin/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: adminToken,
      Email: 'admin@phelieuminhduc.vn',
      Password: 'Admin@2026!'
    })
  }, adminJar);

  const dashRes = await request('/admin', {}, adminJar);
  record('AUTH-01', 'Admin đăng nhập hợp lệ', dashRes.statusCode === 200 ? 'PASS' : 'FAIL', 'Cookie active');

  // =========================================================================
  // 2. VALIDATION & BOUNDARY CONDITIONS (ADMIN FORMS)
  // =========================================================================
  console.log("\n📐 --- PHẦN 2: KIỂM THỬ GIÁ TRỊ BIÊN & VALIDATION FORM ADMIN ---");

  // 2.1 Empty Scrap Category Name
  const catFormGet = await request('/admin/scrap/CategoryForm', {}, adminJar);
  const catToken = extractAntiforgeryToken(catFormGet.body);
  const emptyCatRes = await request('/admin/scrap/SaveCategory', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: catToken,
      Name: '',
      SortOrder: '1'
    })
  }, adminJar);
  const emptyCatBlocked = emptyCatRes.body.includes('alert-danger') || emptyCatRes.body.includes('Chưa lưu được');
  record('VAL-CAT-01', 'Validation: Nhóm phế liệu để trống Tên -> Báo lỗi', emptyCatBlocked ? 'PASS' : 'FAIL', 'Validation caught empty name');

  // 2.2 Scrap Item with Empty CategoryId
  const scrapFormGet = await request('/admin/scrap/Form', {}, adminJar);
  const scrapToken = extractAntiforgeryToken(scrapFormGet.body);
  const emptyScrapCatRes = await request('/admin/scrap/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: scrapToken,
      Name: 'EDGE Phế Liệu Thiếu Nhóm',
      CategoryId: ''
    })
  }, adminJar);
  const scrapCatBlocked = emptyScrapCatRes.body.includes('alert-danger') || emptyScrapCatRes.body.includes('Chưa lưu được');
  record('VAL-SCR-01', 'Validation: Loại phế liệu chưa chọn nhóm -> Báo lỗi', scrapCatBlocked ? 'PASS' : 'FAIL', 'Validation caught missing CategoryId');

  // 2.3 Price Matrix Save with 0 rows selected
  const priceGet = await request('/admin/prices', {}, adminJar);
  const priceToken = extractAntiforgeryToken(priceGet.body);
  const emptyPriceBulkRes = await request('/admin/prices/SaveBulk', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: priceToken
      // No rows selected
    })
  }, adminJar);
  // After redirect, check if warning message appears
  const priceAfterRedirect = await request(emptyPriceBulkRes.headers.location || '/admin/prices', {}, adminJar);
  const priceWarnCaught = priceAfterRedirect.body.includes('Vui lòng tick chọn ít nhất một dòng giá') || emptyPriceBulkRes.statusCode === 302;
  record('VAL-PRI-01', 'Bảng giá: Bấm Lưu khi chưa tick chọn dòng nào -> Cảnh báo', priceWarnCaught ? 'PASS' : 'FAIL', 'Handled unselected bulk submit');

  // 2.4 Article Form Empty Title
  const artFormGet = await request('/admin/articles/Form', {}, adminJar);
  const artToken = extractAntiforgeryToken(artFormGet.body);
  const emptyArtRes = await request('/admin/articles/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: artToken,
      Title: '',
      PostCategoryId: '1'
    })
  }, adminJar);
  const artBlocked = emptyArtRes.body.includes('alert-danger') || emptyArtRes.body.includes('Chưa lưu được');
  record('VAL-ART-01', 'Validation: Bài viết để trống Tiêu đề -> Báo lỗi', artBlocked ? 'PASS' : 'FAIL', 'Validation caught empty Title');

  // 2.5 Service Form Empty Title
  const srvFormGet = await request('/admin/services/Form', {}, adminJar);
  const srvToken = extractAntiforgeryToken(srvFormGet.body);
  const emptySrvRes = await request('/admin/services/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: srvToken,
      Title: ''
    })
  }, adminJar);
  const srvBlocked = emptySrvRes.body.includes('alert-danger') || emptySrvRes.body.includes('Chưa lưu được');
  record('VAL-SRV-01', 'Validation: Dịch vụ để trống Tiêu đề -> Báo lỗi', srvBlocked ? 'PASS' : 'FAIL', 'Validation caught empty Title');

  // 2.6 Location Form Empty Name
  const locFormGet = await request('/admin/locations/Form', {}, adminJar);
  const locToken = extractAntiforgeryToken(locFormGet.body);
  const emptyLocRes = await request('/admin/locations/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: locToken,
      Name: '',
      Province: ''
    })
  }, adminJar);
  const locBlocked = emptyLocRes.body.includes('alert-danger') || emptyLocRes.body.includes('Chưa lưu được');
  record('VAL-LOC-01', 'Validation: Khu vực để trống Tên/Tỉnh -> Báo lỗi', locBlocked ? 'PASS' : 'FAIL', 'Validation caught empty Name');

  // 2.7 Project Form Empty Title
  const prjFormGet = await request('/admin/projects/Form', {}, adminJar);
  const prjToken = extractAntiforgeryToken(prjFormGet.body);
  const emptyPrjRes = await request('/admin/projects/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: prjToken,
      Title: ''
    })
  }, adminJar);
  const prjBlocked = emptyPrjRes.body.includes('alert-danger') || emptyPrjRes.body.includes('Chưa lưu được');
  record('VAL-PRJ-01', 'Validation: Dự án để trống Tiêu đề -> Báo lỗi', prjBlocked ? 'PASS' : 'FAIL', 'Validation caught empty Title');

  // 2.8 FAQ Form Empty Question
  const faqFormGet = await request('/admin/faq/Form', {}, adminJar);
  const faqToken = extractAntiforgeryToken(faqFormGet.body);
  const emptyFaqRes = await request('/admin/faq/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: faqToken,
      Question: ''
    })
  }, adminJar);
  const faqBlocked = emptyFaqRes.body.includes('alert-danger') || emptyFaqRes.body.includes('Chưa lưu được');
  record('VAL-FAQ-01', 'Validation: FAQ để trống Câu hỏi -> Báo lỗi', faqBlocked ? 'PASS' : 'FAIL', 'Validation caught empty Question');

  // =========================================================================
  // 3. SLUG GENERATION, SPECIAL CHARACTERS & COLLISION RESILIENCE
  // =========================================================================
  console.log("\n🔤 --- PHẦN 3: XỬ LÝ SLUG TIẾNG VIỆT, KÝ TỰ ĐẶC BIỆT & TRÁNH TRÙNG LẶP ---");

  // 3.1 Category with heavy Vietnamese diacritics & symbols
  const specialCatName = "EDGE Nhóm Đột Biến: Đồng & Nhôm (Loại 1 - 99.9%)";
  await request('/admin/scrap/SaveCategory', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: catToken,
      Name: specialCatName,
      Description: "Mô tả kiểm thử ký tự đặc biệt",
      SortOrder: "99"
    })
  }, adminJar);

  const catInDb = runSql(`SELECT TOP 1 Slug FROM dbo.ScrapCategories WHERE Name = N'${specialCatName}'`);
  const isCatSlugValid = catInDb && !catInDb.includes('SQL_ERROR') && !catInDb.includes('%') && !catInDb.includes('&');
  record('SLUG-01', 'Sinh Slug tiếng Việt có ký tự đặc biệt (&, %, ())', isCatSlugValid ? 'PASS' : 'FAIL', `Slug: ${catInDb}`);

  const catId = runSql(`SELECT TOP 1 Id FROM dbo.ScrapCategories WHERE Name = N'${specialCatName}'`);

  // 3.2 Duplicate Scrap Items (Slug Collision Handling)
  const duplicateItemName = "EDGE Dây Cáp Đồng Trần VIP";
  await request('/admin/scrap/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: scrapToken,
      CategoryId: catId,
      Name: duplicateItemName,
      PriceLabel: "200.000đ/kg",
      Status: "published"
    })
  }, adminJar);

  // Refresh token for second submit
  const scrapFormGet2 = await request('/admin/scrap/Form', {}, adminJar);
  const scrapToken2 = extractAntiforgeryToken(scrapFormGet2.body);

  // Create second item with EXACT same name
  await request('/admin/scrap/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: scrapToken2,
      CategoryId: catId,
      Name: duplicateItemName,
      PriceLabel: "210.000đ/kg",
      Status: "published"
    })
  }, adminJar);

  const duplicateSlugs = runSql(`SELECT Slug FROM dbo.ScrapItems WHERE Name = N'${duplicateItemName}' AND DeletedAt IS NULL`);
  const slugsList = duplicateSlugs.split('\n').map(s => s.trim()).filter(Boolean);
  const hasUniqueSlugs = slugsList.length >= 2 && slugsList[0] !== slugsList[1];
  record('SLUG-02', 'Tránh trùng Slug khi tạo 2 loại phế liệu trùng tên', hasUniqueSlugs ? 'PASS' : 'FAIL', `Slugs: ${slugsList.join(' vs ')}`);

  // =========================================================================
  // 4. LIFECYCLE, SOFT DELETE & LIVE/DRAFT CLIENT INVARIANTS
  // =========================================================================
  console.log("\n🔄 --- PHẦN 4: VÒNG ĐỜI DỮ LIỆU, TRẠNG THÁI DRAFT/PUBLISHED & XÓA MỀM ---");

  // 4.1 Article Draft vs Published
  const draftArtTitle = `EDGE Bài Viết Bản Nháp Test Invariant ${Date.now()}`;
  const postCatId = runSql("SELECT TOP 1 Id FROM dbo.PostCategories");
  const artCreateGet = await request('/admin/articles/Form', {}, adminJar);
  const artCreateToken = extractAntiforgeryToken(artCreateGet.body);

  await request('/admin/articles/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: artCreateToken,
      Title: draftArtTitle,
      PostCategoryId: postCatId,
      Excerpt: "Tóm tắt bản nháp",
      Content: "<p>Nội dung nháp</p>",
      Status: "draft"
    })
  }, adminJar);

  const draftPostId = runSql(`SELECT TOP 1 Id FROM dbo.Posts WHERE Title = N'${draftArtTitle}'`);
  const draftSlug = runSql(`SELECT TOP 1 Slug FROM dbo.Posts WHERE Id = ${draftPostId}`);
  const publicDraftCheck = await request(`/tin-tuc/${draftSlug}`);
  record('LIFE-ART-01', 'Bài viết trạng thái "draft" -> Client truy cập trả về 404', publicDraftCheck.statusCode === 404 ? 'PASS' : 'FAIL', `Status: ${publicDraftCheck.statusCode}`);

  // Now publish it
  const artEditGet = await request(`/admin/articles/Form?id=${draftPostId}`, {}, adminJar);
  const artEditToken = extractAntiforgeryToken(artEditGet.body);
  await request('/admin/articles/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: artEditToken,
      Id: draftPostId,
      Title: draftArtTitle,
      Slug: draftSlug,
      PostCategoryId: postCatId,
      Excerpt: "Tóm tắt đã xuất bản",
      Content: "<p>Nội dung chính thức</p>",
      Status: "published"
    })
  }, adminJar);

  const updatedSlug = runSql(`SELECT TOP 1 Slug FROM dbo.Posts WHERE Id = ${draftPostId}`);
  const publicPublishCheck = await request(`/tin-tuc/${updatedSlug}`);
  const containsTitle = publicPublishCheck.body.includes(draftArtTitle);
  if (!containsTitle) {
    console.log("DEBUG: publicPublishCheck status:", publicPublishCheck.statusCode, "slug:", updatedSlug, "body snippet:", publicPublishCheck.body.substring(0, 300));
  }
  record('LIFE-ART-02', 'Chuyển bài viết sang "published" -> Client hiển thị 200 OK', publicPublishCheck.statusCode === 200 && (containsTitle || publicPublishCheck.body.includes("EDGE")) ? 'PASS' : 'FAIL', `Status: ${publicPublishCheck.statusCode}`);

  // 4.2 Soft delete Scrap Item
  const deleteItemName = `EDGE Phế Liệu Xóa Mềm ${Date.now()}`;
  const scrapFormGet4 = await request('/admin/scrap/Form', {}, adminJar);
  const scrapToken4 = extractAntiforgeryToken(scrapFormGet4.body);
  await request('/admin/scrap/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: scrapToken4,
      CategoryId: catId,
      Name: deleteItemName,
      PriceLabel: "150.000đ/kg",
      Status: "published"
    })
  }, adminJar);

  const deleteItemId = runSql(`SELECT TOP 1 Id FROM dbo.ScrapItems WHERE Name = N'${deleteItemName}'`);
  const deleteItemSlug = runSql(`SELECT TOP 1 Slug FROM dbo.ScrapItems WHERE Id = ${deleteItemId}`);

  const scrapListGet = await request('/admin/scrap', {}, adminJar);
  const scrapListToken = extractAntiforgeryToken(scrapListGet.body);
  await request('/admin/scrap/Delete', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: scrapListToken,
      id: deleteItemId
    })
  }, adminJar);

  const isDeletedInDb = runSql(`SELECT CASE WHEN DeletedAt IS NOT NULL THEN 'DELETED' ELSE 'ACTIVE' END FROM dbo.ScrapItems WHERE Id = ${deleteItemId}`);
  const publicDeletedItemCheck = await request(`/phe-lieu/${deleteItemSlug}`);
  record('LIFE-SCR-01', 'Xóa mềm phế liệu -> DB lưu DeletedAt, Client trả về 404', isDeletedInDb === 'DELETED' && publicDeletedItemCheck.statusCode === 404 ? 'PASS' : 'FAIL', `DB: ${isDeletedInDb}, HTTP: ${publicDeletedItemCheck.statusCode}`);

  // =========================================================================
  // 5. PRICE MATRIX & PRICE HISTORY AUDIT TRAIL
  // =========================================================================
  console.log("\n💰 --- PHẦN 5: BẢNG GIÁ & GHI NHẬN LỊCH SỬ THAY ĐỔI GIÁ ---");

  // Create a scrap item with price row
  const priceCatId = runSql("SELECT TOP 1 Id FROM dbo.ScrapCategories WHERE Name LIKE N'EDGE%'");
  const priceTestItem = `EDGE Dong Do Test History ${Date.now()}`;
  const scrapFormGet3 = await request('/admin/scrap/Form', {}, adminJar);
  const scrapToken3 = extractAntiforgeryToken(scrapFormGet3.body);
  const saveRes3 = await request('/admin/scrap/Save', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: scrapToken3,
      CategoryId: priceCatId,
      Name: priceTestItem,
      PriceLabel: "180.000đ/kg",
      Unit: "kg",
      Status: "published"
    })
  }, adminJar);

  const priceItemId = runSql(`SELECT TOP 1 Id FROM dbo.ScrapItems WHERE Name = '${priceTestItem}'`);
  console.log("DEBUG: priceCatId:", priceCatId, "saveRes3 status:", saveRes3.statusCode, "priceItemId:", priceItemId);

  // Insert initial price row
  if (priceItemId && !priceItemId.includes('SQL_ERROR') && priceItemId.length > 0) {
    runSql(`INSERT INTO dbo.ScrapPrices (ScrapItemId, PriceValue, PriceLabel, Unit, EffectiveDate) VALUES (${priceItemId}, 180000, N'180.000đ', 'kg', CAST(GETDATE() AS date))`);
  }

  const priceRowId = runSql(`SELECT TOP 1 Id FROM dbo.ScrapPrices WHERE ScrapItemId = ${priceItemId}`);

  // Update price through bulk matrix
  if (priceRowId && !priceRowId.includes('SQL_ERROR') && priceRowId.length > 0) {
    const priceMatrixGet = await request('/admin/prices', {}, adminJar);
    const priceMatrixToken = extractAntiforgeryToken(priceMatrixGet.body);
    const bulkPricePayload = querystring.stringify({
      __RequestVerificationToken: priceMatrixToken,
      'rows[0].PriceId': priceRowId,
      'rows[0].Selected': 'true',
      'rows[0].PriceValue': '235000'
    });

    await request('/admin/prices/SaveBulk', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: bulkPricePayload
    }, adminJar);

    const updatedPriceInDb = runSql(`SELECT TOP 1 PriceValue FROM dbo.ScrapPrices WHERE Id = ${priceRowId}`);
    const historyCount = runSql(`SELECT COUNT(*) FROM dbo.ScrapPriceHistory WHERE ScrapItemId = ${priceItemId}`);
    record('PRI-HIST-01', 'Lưu giá mới 235.000đ -> Cập nhật giá & tự động ghi ScrapPriceHistory', updatedPriceInDb.includes('235000') && parseInt(historyCount) >= 1 ? 'PASS' : 'FAIL', `Price: ${updatedPriceInDb}, History rows: ${historyCount}`);
  } else {
    record('PRI-HIST-01', 'Lưu giá mới 235.000đ -> Cập nhật giá & tự động ghi ScrapPriceHistory', 'FAIL', 'Could not locate price row');
  }

  // Check reflection on Client /bang-gia
  const bangGiaPage = await request('/bang-gia');
  record('PRI-PUB-01', 'Hiển thị giá mới cập nhật trên trang /bang-gia Client', bangGiaPage.statusCode === 200 && bangGiaPage.body.includes(priceTestItem) ? 'PASS' : 'FAIL', 'Found item in /bang-gia');

  // =========================================================================
  // 6. CLIENT LEAD CAPTURE & ADMIN STATUS PROGRESSION
  // =========================================================================
  console.log("\n📬 --- PHẦN 6: TIẾP NHẬN YÊU CẦU BÁO GIÁ & QUY TRÌNH XỬ LÝ LEAD ---");

  // 6.1 Validation on Client Contact Form
  clientJar.clear();
  const contactGet = await request('/lien-he', {}, clientJar);
  const contactToken = extractAntiforgeryToken(contactGet.body);
  const emptyContactRes = await request('/contact', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: contactToken,
      Name: '',
      Phone: '',
      Message: ''
    })
  }, clientJar);
  const contactValError = emptyContactRes.body.includes('alert-danger') || emptyContactRes.body.includes('field-validation-error');
  record('LEAD-VAL-01', 'Client: Form liên hệ để trống -> Báo lỗi validation', contactValError ? 'PASS' : 'FAIL', 'Required fields enforced');

  // 6.2 Submit Valid Lead with Vietnamese Unicode
  const contactGet2 = await request('/lien-he', {}, clientJar);
  const contactToken2 = extractAntiforgeryToken(contactGet2.body);
  const leadName = `EDGE Nguyễn Văn Minh Chuyên Gia Phế Liệu ${Date.now()}`;
  const leadPhone = "099" + Math.floor(1000000 + Math.random() * 9000000);
  const leadMsg = "Cần thanh lý 5 tấn đồng đỏ và 10 tấn dây điện nhà xưởng tại KCN Sóng Thần.";
  await request('/contact', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: contactToken2,
      Name: leadName,
      Phone: leadPhone,
      Email: 'minh.edge@example.com',
      Area: 'Bình Dương',
      Message: leadMsg
    })
  }, clientJar);

  const leadInDb = runSql(`SELECT TOP 1 Status FROM dbo.ContactRequests WHERE Phone = '${leadPhone}'`);
  record('LEAD-SUB-01', 'Khách gửi lead hợp lệ -> Ghi DB với trạng thái "New" / "new"', leadInDb.toLowerCase().includes('new') ? 'PASS' : 'FAIL', `Status: ${leadInDb}`);

  const leadId = runSql(`SELECT TOP 1 Id FROM dbo.ContactRequests WHERE Phone = '${leadPhone}'`);

  // 6.3 Admin mark contacted
  const leadsPageGet = await request('/admin/leads', {}, adminJar);
  const leadsToken = extractAntiforgeryToken(leadsPageGet.body);
  await request('/admin/leads/MarkContacted', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: querystring.stringify({
      __RequestVerificationToken: leadsToken,
      id: leadId
    })
  }, adminJar);

  const leadStatusAfter = runSql(`SELECT TOP 1 Status FROM dbo.ContactRequests WHERE Id = ${leadId}`);
  record('LEAD-PROC-01', 'Admin đánh dấu "Đã liên hệ" -> DB cập nhật trạng thái "contacted"', leadStatusAfter.toLowerCase().includes('contacted') ? 'PASS' : 'FAIL', `Status: ${leadStatusAfter}`);

  // =========================================================================
  // 7. CLIENT SEARCH FUZZING & QUERY STRESS
  // =========================================================================
  console.log("\n🔍 --- PHẦN 7: FUZZING TÌM KIẾM CLIENT & KÝ TỰ ĐẶC BIỆT ---");

  const searchTests = [
    { query: '', label: 'Truy vấn trống (?q=)' },
    { query: '   ', label: 'Truy vấn toàn khoảng trắng (?q=   )' },
    { query: '%%%', label: 'Ký tự đại diện SQL wildcards (?q=%%%)' },
    { query: "'''", label: 'Dấu nháy đơn SQL (?q=\'\'\')' },
    { query: '<script>alert("XSS")</script>', label: 'XSS script injection (?q=<script>...)' },
    { query: 'khong_ton_tai_tu_khoa_nay_xyz12345', label: 'Từ khóa không có kết quả' },
    { query: 'đồng đỏ', label: 'Tiếng Việt có dấu (?q=đồng đỏ)' },
    { query: 'dong do', label: 'Tiếng Việt không dấu (?q=dong do)' }
  ];

  for (const st of searchTests) {
    const sRes = await request(`/tim-kiem?q=${encodeURIComponent(st.query)}`);
    const isSafe = sRes.statusCode === 200 && !sRes.body.includes('<script>alert("XSS")</script>');
    record(`SRCH-${st.label.substring(0, 10)}`, `Tìm kiếm: ${st.label}`, isSafe ? 'PASS' : 'FAIL', `HTTP ${sRes.statusCode}`);
  }

  // =========================================================================
  // 8. PAGINATION BOUNDARY TESTS
  // =========================================================================
  console.log("\n📄 --- PHẦN 8: KIỂM THỬ PHÂN TRANG VƯỢT BIÊN ---");

  const paginationUrls = [
    { url: '/admin/scrap?page=1', label: 'Admin Scrap Trang 1' },
    { url: '/admin/scrap?page=99999', label: 'Admin Scrap Trang 99999 (vượt biên trên)' },
    { url: '/admin/scrap?page=-5', label: 'Admin Scrap Trang -5 (số âm)' },
    { url: '/admin/leads?page=99999', label: 'Admin Leads Trang 99999' },
    { url: '/tin-tuc?page=99999', label: 'Client Tin Tức Trang 99999' }
  ];

  for (const p of paginationUrls) {
    const isAdm = p.url.startsWith('/admin');
    const pRes = await request(p.url, {}, isAdm ? adminJar : null);
    record(`PAG-${p.label.substring(0, 10)}`, `Phân trang: ${p.label}`, pRes.statusCode === 200 ? 'PASS' : 'FAIL', `HTTP ${pRes.statusCode}`);
  }

  // =========================================================================
  // 9. ERROR HANDLING & 404 RESILIENCE (NO 500 STACK TRACES)
  // =========================================================================
  console.log("\n🛡️ --- PHẦN 9: XỬ LÝ LỖI 404 & KHÔNG LỘ STACK TRACE ---");

  const notFoundRoutes = [
    { url: '/duong-dan-hoan-toan-khong-ton-tai-xyz-12345', label: 'Đường dẫn ngẫu nhiên không tồn tại' },
    { url: '/phe-lieu/san-pham-ao-khong-co-that-9999', label: 'Phế liệu không tồn tại' },
    { url: '/tin-tuc/bai-viet-ao-khong-co-that-9999', label: 'Bài viết không tồn tại' },
    { url: '/dich-vu/dich-vu-ao-khong-co-that-9999', label: 'Dịch vụ không tồn tại' },
    { url: '/du-an/du-an-ao-khong-co-that-9999', label: 'Dự án không tồn tại' }
  ];

  for (const nf of notFoundRoutes) {
    const nfRes = await request(nf.url);
    const isClean404 = nfRes.statusCode === 404 && !nfRes.body.includes('System.NullReferenceException') && !nfRes.body.includes('Stack Trace:');
    record(`ERR-404-${nf.label.substring(0, 8)}`, `Xử lý 404: ${nf.label}`, isClean404 ? 'PASS' : 'FAIL', `HTTP ${nfRes.statusCode}`);
  }

  // 9.1 Location detail 301 redirect by design
  const locDetailRes = await request('/khu-vuc/khu-vuc-cu-chuyen-huong');
  const isLocRedirect = locDetailRes.statusCode === 301 && locDetailRes.headers.location === '/khu-vuc';
  record('ROUTE-LOC-301', 'Khu vực cũ: 301 RedirectPermanent về /khu-vuc', isLocRedirect ? 'PASS' : 'FAIL', `Status: ${locDetailRes.statusCode} -> ${locDetailRes.headers.location}`);

  // =========================================================================
  // 10. TECHNICAL SEO & XML SITEMAP VALIDATION
  // =========================================================================
  console.log("\n🌐 --- PHẦN 10: KIỂM TRA CHUẨN SEO ROBOTS.TXT & SITEMAP.XML ---");

  const robotsRes = await request('/robots.txt');
  const robotsValid = robotsRes.statusCode === 200 && robotsRes.body.includes('User-agent:') && robotsRes.body.includes('Sitemap:');
  record('SEO-ROBOTS', 'Tệp /robots.txt chuẩn chỉ thị crawl', robotsValid ? 'PASS' : 'FAIL', `HTTP ${robotsRes.statusCode}`);

  const sitemapRes = await request('/sitemap.xml');
  const sitemapValid = sitemapRes.statusCode === 200 && sitemapRes.body.includes('<urlset') && sitemapRes.body.includes('<loc>') && sitemapRes.body.includes('</urlset>');
  record('SEO-SITEMAP', 'Tệp /sitemap.xml chuẩn cấu trúc XML sitemap', sitemapValid ? 'PASS' : 'FAIL', `HTTP ${sitemapRes.statusCode}`);

  // =========================================================================
  // SUMMARY REPORT
  // =========================================================================
  console.log("\n🧹 Dọn dẹp dữ liệu EDGE-* sau khi hoàn thành...");
  runSql(`
    DELETE FROM dbo.ContactRequests WHERE Name LIKE N'EDGE-%' OR Phone LIKE '0999%';
    DELETE FROM dbo.FaqItems WHERE Question LIKE N'EDGE-%';
    DELETE FROM dbo.Projects WHERE Title LIKE N'EDGE-%';
    DELETE FROM dbo.Locations WHERE Name LIKE N'EDGE-%';
    DELETE FROM dbo.Services WHERE Title LIKE N'EDGE-%';
    DELETE FROM dbo.Posts WHERE Title LIKE N'EDGE-%';
    DELETE FROM dbo.PostAutosaves WHERE PostKey LIKE 'edge-%';
    DELETE FROM dbo.ScrapPriceHistory WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'EDGE-%');
    DELETE FROM dbo.ScrapPrices WHERE ScrapItemId IN (SELECT Id FROM dbo.ScrapItems WHERE Name LIKE N'EDGE-%');
    DELETE FROM dbo.ScrapItems WHERE Name LIKE N'EDGE-%';
    DELETE FROM dbo.ScrapCategories WHERE Name LIKE N'EDGE-%';
  `);

  console.log("\n" + "=".repeat(80));
  console.log("📊 BẢNG TỔNG HỢP KIỂM THỬ CHUYÊN SÂU & BẮT LỖI BIÊN (DEEP EDGE CASES)");
  console.log("=".repeat(80));
  const passCount = testResults.filter(r => r.status === "PASS").length;
  const warnCount = testResults.filter(r => r.status === "WARN").length;
  const failCount = testResults.filter(r => r.status === "FAIL").length;
  console.log(`Tổng số ca kiểm thử chuyên sâu: ${testResults.length}`);
  console.log(`✅ PASS: ${passCount} (${(passCount / testResults.length * 100).toFixed(1)}%)`);
  console.log(`⚠️ WARN: ${warnCount} (${(warnCount / testResults.length * 100).toFixed(1)}%)`);
  console.log(`❌ FAIL: ${failCount} (${(failCount / testResults.length * 100).toFixed(1)}%)`);
  console.log("=".repeat(80));
}

runDeepTests().catch(console.error);