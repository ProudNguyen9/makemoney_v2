/**
 * Generator ảnh placeholder SVG cho scrap-template.
 * Mỗi ảnh có đúng aspect ratio theo DESIGN_SYSTEM.md và ghi chú ảnh thật cần thay thế.
 * Chạy: node tools/generate-placeholders.js
 */
const fs = require('fs');
const path = require('path');

const ROOT = path.join(__dirname, '..', 'assets', 'images');

/** [filePath, width, height, tiêu đề, ghi chú ảnh thật cần thay] */
const IMAGES = [
  // HERO — 16:10
  ['hero/hero-main.svg', 1600, 1000, 'ẢNH CHÍNH — KHO PHẾ LIỆU', 'Thay bằng ảnh thật: sân kho / phân loại phế liệu, 1600×1000 (16:10), ánh sáng tự nhiên'],
  ['hero/hero-01.svg', 1280, 800, 'XE TẢI THU GOM', 'Thay bằng ảnh thật: xe tải tại công trình thu gom, 1280×800'],
  ['hero/hero-02.svg', 1280, 800, 'CÂN PHẾ LIỆU', 'Thay bằng ảnh thật: cân điện tử / cân xe, 1280×800'],
  // SCRAP — 4:3
  ['scrap/scrap-copper.svg', 1200, 900, 'PHẾ LIỆU ĐỒNG', 'Thay bằng ảnh thật: dây đồng / đồng đỏ phân loại, 1200×900 (4:3)'],
  ['scrap/scrap-iron.svg', 1200, 900, 'PHẾ LIỆU SẮT THÉP', 'Thay bằng ảnh thật: sắt công trình / thép vụn, 1200×900'],
  ['scrap/scrap-aluminum.svg', 1200, 900, 'PHẾ LIỆU NHÔM', 'Thay bằng ảnh thật: nhôm thanh / nhôm vụn, 1200×900'],
  ['scrap/scrap-stainless.svg', 1200, 900, 'PHẾ LIỆU INOX', 'Thay bằng ảnh thật: inox 304 / phế inox, 1200×900'],
  ['scrap/scrap-cable.svg', 1200, 900, 'DÂY ĐIỆN — ĐỒNG CÁP', 'Thay bằng ảnh thật: dây điện, dây cáp đồng, 1200×900'],
  ['scrap/scrap-motor.svg', 1200, 900, 'MOTOR — MÁY MÓC CŨ', 'Thay bằng ảnh thật: motor điện, máy móc cũ, 1200×900'],
  ['scrap/scrap-board.svg', 1200, 900, 'BO MẠCH — TỬ ĐIỆN', 'Thay bằng ảnh thật: bo mạch, tủ điện cũ, 1200×900'],
  ['scrap/scrap-misc.svg', 1200, 900, 'PHẾ LIỆU KHÁC', 'Thay bằng ảnh thật: chì, kẽm, niken, giấy, nhựa..., 1200×900'],
  // COMPANY — 3:2
  ['company/company-yard.svg', 1200, 800, 'SÂN KHO', 'Thay bằng ảnh thật: toàn cảnh sân kho, 1200×800 (3:2)'],
  ['company/company-truck.svg', 1200, 800, 'ĐỘI XE', 'Thay bằng ảnh thật: đội xe tải / cẩu, 1200×800'],
  ['company/company-team.svg', 1200, 800, 'ĐỘI NGŨ NHÂN VIÊN', 'Thay bằng ảnh thật: nhân viên tác nghiệp (đủ bảo hộ), 1200×800'],
  ['company/company-warehouse.svg', 1200, 800, 'NHÀ XƯỞNG', 'Thay bằng ảnh thật: nhà xưởng / phân xưởng, 1200×800'],
  ['company/company-scale.svg', 1200, 800, 'CÂN ĐIỆN TỬ', 'Thay bằng ảnh thật: khu cân — minh bạch cân đo, 1200×800'],
  // PROJECTS — 16:10
  ['projects/project-01-cover.svg', 1280, 800, 'DỰ ÁN 01 — THÁO DỠ NHÀ XƯỞNG', 'Thay bằng ảnh thật: dự án tháo dỡ nhà xưởng, 1280×800 (16:10)'],
  ['projects/project-02-cover.svg', 1280, 800, 'DỰ ÁN 02 — THANH LÝ CÔNG TRÌNH', 'Thay bằng ảnh thật: dự án thu gom công trình, 1280×800'],
  ['projects/project-03-cover.svg', 1280, 800, 'DỰ ÁN 03 — MÁY MÓC SẢN XUẤT', 'Thay bằng ảnh thật: dự án mua máy móc cũ, 1280×800'],
  ['projects/project-04-cover.svg', 1280, 800, 'DỰ ÁN 04 — DÂY CHUYỀN SẢN XUẤT', 'Thay bằng ảnh thật: dự án tháo dỡ dây chuyền, 1280×800'],
  ['projects/project-05-cover.svg', 1280, 800, 'DỰ ÁN 05 — LÔ ĐỒNG LỚN', 'Thay bằng ảnh thật: dự án thu mua lô đồng lớn, 1280×800'],
  ['projects/project-06-cover.svg', 1280, 800, 'DỰ ÁN 06 — THU GOM ĐỊNH KỲ', 'Thay bằng ảnh thật: hợp tác thu gom định kỳ, 1280×800'],
  ['projects/project-07-cover.svg', 1280, 800, 'DỰ ÁN 07', 'Thay bằng ảnh thật, 1280×800'],
  ['projects/project-08-cover.svg', 1280, 800, 'DỰ ÁN 08', 'Thay bằng ảnh thật, 1280×800'],
  // NEWS / ARTICLE — 16:9
  ['news/news-01.svg', 1200, 675, 'BÀI VIẾT 01 — BẢNG GIÁ', 'Thay bằng ảnh thật minh họa bài viết, 1200×675 (16:9)'],
  ['news/news-02.svg', 1200, 675, 'BÀI VIẾT 02 — TIN NGÀNH', 'Thay bằng ảnh thật minh họa bài viết, 1200×675'],
  ['news/news-03.svg', 1200, 675, 'BÀI VIẾT 03 — KINH NGHIỆM', 'Thay bằng ảnh thật minh họa bài viết, 1200×675'],
  ['news/news-04.svg', 1200, 675, 'BÀI VIẾT 04', 'Thay bằng ảnh thật minh họa bài viết, 1200×675'],
  ['news/news-05.svg', 1200, 675, 'BÀI VIẾT 05', 'Thay bằng ảnh thật minh họa bài viết, 1200×675'],
  ['news/news-06.svg', 1200, 675, 'BÀI VIẾT 06', 'Thay bằng ảnh thật minh họa bài viết, 1200×675'],
  // LOCATIONS — 16:9
  ['locations/location-dongnai.svg', 1200, 675, 'ĐỒNG NAI', 'Thay bằng ảnh thật: cảnh thu gom tại Đồng Nai, 1200×675'],
  ['locations/location-hcm.svg', 1200, 675, 'TP. HỒ CHÍ MINH', 'Thay bằng ảnh thật: cảnh thu gom tại TP.HCM, 1200×675'],
  ['locations/location-binhduong.svg', 1200, 675, 'BÌNH DƯƠNG', 'Thay bằng ảnh thật: cảnh thu gom tại Bình Dương, 1200×675'],
  ['locations/location-map.svg', 1600, 900, 'BẢN ĐỒ PHỦ KÍNH', 'Thay bằng ảnh/Google Map embed khu vực hoạt động, 1600×900'],
  // LOGO
  ['logo/logo.svg', 480, 120, 'LOGO', 'Thay bằng logo công ty (SVG ưu tiên), 480×120'],
  ['logo/logo-footer.svg', 480, 120, 'LOGO FOOTER (BẢN TRẮNG)', 'Thay bằng logo bản trắng ngang, 480×120'],
];

function gcd(a, b) { return b === 0 ? a : gcd(b, a % b); }
function ratio(w, h) { const g = gcd(w, h); return `${w / g}:${h / g}`; }

function svg(file, w, h, title, note) {
  const r = ratio(w, h);
  const fs9 = Math.round(w / 42);
  const fs7 = Math.round(w / 55);
  const fs5 = Math.round(w / 78);
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}" role="img" aria-label="${title} — placeholder">
  <defs>
    <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse">
      <path d="M40 0H0V40" fill="none" stroke="#3A414B" stroke-width="1"/>
    </pattern>
    <pattern id="hazard" width="24" height="24" patternUnits="userSpaceOnUse" patternTransform="rotate(45)">
      <rect width="12" height="24" fill="#E4570F"/>
      <rect x="12" width="12" height="24" fill="#F5B301"/>
    </pattern>
  </defs>
  <rect width="${w}" height="${h}" fill="#242930"/>
  <rect width="${w}" height="${h}" fill="url(#grid)" opacity=".35"/>
  <rect x="0" y="0" width="${w}" height="14" fill="url(#hazard)" opacity=".9"/>
  <rect x="0" y="${h - 14}" width="${w}" height="14" fill="url(#hazard)" opacity=".9"/>
  <g fill="#E8EAED" font-family="Arial, Helvetica, sans-serif" text-anchor="middle">
    <text x="${w / 2}" y="${h / 2 - fs9 * 0.4}" font-size="${fs9}" font-weight="700" letter-spacing="2">${title}</text>
    <text x="${w / 2}" y="${h / 2 + fs7 * 1.1}" font-size="${fs7}" fill="#9AA3AE" letter-spacing="3">${w}×${h} — TỈ LỆ ${r}</text>
    <text x="${w / 2}" y="${h / 2 + fs7 * 3.2}" font-size="${fs5}" fill="#E4570F">${note}</text>
  </g>
</svg>
`;
}

let count = 0;
for (const [file, w, h, title, note] of IMAGES) {
  const full = path.join(ROOT, file);
  fs.mkdirSync(path.dirname(full), { recursive: true });
  fs.writeFileSync(full, svg(file, w, h, title, note));
  count++;
}
console.log(`Đã tạo ${count} ảnh placeholder SVG.`);
