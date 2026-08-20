/* =============================================================================
 * NAVIGATION.JS — menu: dropdown desktop (hover + keyboard), offcanvas mobile
 * ===========================================================================*/
(function () {
  'use strict';

  /* ---------------------------------------------------------------
   * Desktop dropdown (Bootstrap đã xử lý click; phần này bổ trợ hover +
   * giữ aria-expanded chuẩn khi thao tác chuột)
   * --------------------------------------------------------------- */
  function initDesktopDropdown() {
    var items = document.querySelectorAll('.nav-main .has-drop');
    if (!items.length) return;

    items.forEach(function (li) {
      var toggle = li.querySelector('[data-bs-toggle="dropdown"]');
      if (!toggle) return;

      li.addEventListener('mouseenter', function () {
        if (window.matchMedia('(min-width: 992px)').matches) {
          bootstrap.Dropdown.getOrCreateInstance(toggle).show();
        }
      });
      li.addEventListener('mouseleave', function () {
        if (window.matchMedia('(min-width: 992px)').matches) {
          bootstrap.Dropdown.getOrCreateInstance(toggle).hide();
        }
      });
    });
  }

  /* ---------------------------------------------------------------
   * Mobile offcanvas menu — accordion submenu + khoá cuộn nền
   * --------------------------------------------------------------- */
  function initMobileMenu() {
    var menu = document.getElementById('mobileMenu');
    if (!menu) return;

    // Mở/đóng submenu
    menu.querySelectorAll('.mobile-link[data-sub]').forEach(function (btn) {
      btn.addEventListener('click', function (e) {
        e.preventDefault();
        var li = btn.closest('li');
        var expanded = li.classList.toggle('open');
        btn.setAttribute('aria-expanded', expanded ? 'true' : 'false');
      });
    });
  }

  /* ---------------------------------------------------------------
   * Đánh dấu link đang active theo URL hiện tại
   * (Backend nên render class .active trực tiếp; đây là fallback cho template tĩnh)
   * --------------------------------------------------------------- */
  function initActiveLink() {
    var path = location.pathname.split('/').pop() || 'index.html';
    document.querySelectorAll('.nav-main a, .mobile-nav a').forEach(function (a) {
      var href = (a.getAttribute('href') || '').split('#')[0];
      if (href && href === path) {
        var topLink = a.closest('li');
        if (topLink) {
          // chỉ đánh dấu cấp 1 của desktop nav
          if (a.classList.contains('nav-link') || a.classList.contains('mobile-link')) {
            a.classList.add('active');
          }
        }
      }
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initDesktopDropdown();
    initMobileMenu();
    initActiveLink();
  });
})();
