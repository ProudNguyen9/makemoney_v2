# Responsive Audit After View Split

Date: 2026-08-21

## Scope

- Compared ASP.NET Core MVC output in `codezone` against the static source template in `scrap-template`.
- Public pages checked: Home, Contact, Prices, Scrap list/detail/category route, News list/detail, Services list/detail, Projects list/detail, Locations list/detail.
- Admin pages were not included in this pass because the plan prioritizes public responsive first unless admin is explicitly in scope.

## Verification

- Build: `dotnet build .\codezone\ScrapWebsite.csproj --no-restore` passed with 0 warnings and 0 errors.
- Runtime targets:
  - Razor app: `http://localhost:5107`
  - Static template: `http://localhost:5108`
- Viewports checked:
  - `1440x900`
  - `1366x768`
  - `1024x768`
  - `768x1024`
  - `430x932`
  - `390x844`
  - `375x667`

## Findings

- Blockers: none found that are regressions from the view split.
- Needs fix: none found that differ from the template.
- Accepted template-matching behavior:
  - Price tables overflow horizontally inside their intended scroll containers on mobile.
  - Swiper project gallery keeps off-screen slides outside the viewport as part of carousel behavior.
  - The prices page mobile hero shows text embedded in the SVG placeholder background, matching the static template.
  - The prices category nav can extend horizontally on narrow mobile, matching the static template behavior.

## Component Checks

- `_Layout` loads the expected public CSS stack: Bootstrap, Bootstrap Icons, Swiper, `main.css`, and `responsive.css`.
- `codezone/wwwroot/assets/css/responsive.css` matches `scrap-template/assets/css/responsive.css`.
- Shared chrome renders through `Header` and `Footer` view components, which delegate to `_Header` and `_Footer`.
- `QuickQuoteForm` preserves unique IDs through `IdSuffix`; rendered forms passed duplicate-id and label-target checks.
- `PageHero` and `FinalCta` preserve the template class structure and responsive hooks.

## Screenshots

- `tools/responsive-audit/home-1440x900.png`
- `tools/responsive-audit/home-430x932.png`
- `tools/responsive-audit/home-375x667.png`
- `tools/responsive-audit/contact-1440x900.png`
- `tools/responsive-audit/contact-430x932.png`
- `tools/responsive-audit/contact-375x667.png`
- `tools/responsive-audit/prices-1440x900.png`
- `tools/responsive-audit/prices-430x932.png`
- `tools/responsive-audit/prices-375x667.png`
- `tools/responsive-audit/template-prices-375x667.png`

