/* =============================================================================
 * ADMIN.JS — hành vi khu vực quản trị: sidebar, submenu, check-all,
 * media grid/list toggle, preview modal. Desktop-first, mobile usable.
 * ===========================================================================*/
(function () {
  'use strict';

  /* ---------------------------------------------------------------
   * Sidebar mobile: toggle + overlay
   * --------------------------------------------------------------- */
  function initSidebar() {
    var sidebar = document.querySelector('.admin-sidebar');
    var toggle = document.querySelector('.at-toggle');
    var overlay = document.querySelector('.admin-overlay');
    var compactBtn = document.querySelector('.sidebar-collapse-toggle, .sidebar-toggle');
    if (!sidebar || !toggle) return;

    var close = function () {
      sidebar.classList.remove('is-open');
      if (overlay) overlay.classList.remove('show');
      toggle.setAttribute('aria-expanded', 'false');
    };

    toggle.addEventListener('click', function () {
      var open = sidebar.classList.toggle('is-open');
      if (overlay) overlay.classList.toggle('show', open);
      toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
    if (overlay) overlay.addEventListener('click', close);

    if (compactBtn) {
      compactBtn.addEventListener('click', function () {
        var collapsed = sidebar.classList.toggle('is-collapsed');
        var icon = compactBtn.querySelector('i');
        if (icon) {
          icon.className = collapsed ? 'bi bi-chevron-right' : 'bi bi-chevron-left';
        }
        compactBtn.setAttribute(
          'aria-label',
          collapsed ? 'Mở rộng menu quản trị' : 'Thu gọn menu quản trị'
        );
      });
    }

    if (sidebar.classList.contains('is-collapsed') && compactBtn) {
      var initialIcon = compactBtn.querySelector('i');
      if (initialIcon) initialIcon.className = 'bi bi-chevron-right';
      compactBtn.setAttribute('aria-label', 'Mở rộng menu quản trị');
    }
  }

  /* ---------------------------------------------------------------
   * Form search topbar — chặn submit (template-only)
   * --------------------------------------------------------------- */
  function initTopbarSearch() {
    document.querySelectorAll('.at-search').forEach(function (form) {
      form.addEventListener('submit', function (e) { e.preventDefault(); });
    });
  }

  /* ---------------------------------------------------------------
   * Submenu nhóm trong sidebar (mục có .nav-group-btn)
   * --------------------------------------------------------------- */
  function initSidebarSub() {
    document.querySelectorAll('.admin-sidebar .nav-group-btn').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var li = btn.closest('li');
        var open = li.classList.toggle('open');
        btn.setAttribute('aria-expanded', open ? 'true' : 'false');
      });
    });
  }

  /* ---------------------------------------------------------------
   * Check-all trong bảng
   * --------------------------------------------------------------- */
  function initCheckAll() {
    document.querySelectorAll('[data-check-all]').forEach(function (master) {
      var table = master.closest('table');
      if (!table) return;

      master.addEventListener('change', function () {
        table.querySelectorAll('tbody input[type="checkbox"]').forEach(function (cb) {
          cb.checked = master.checked;
        });
      });
    });
  }

  /* ---------------------------------------------------------------
   * Media library: chuyển grid/list
   * --------------------------------------------------------------- */
  function initMediaToggle() {
    var grid = document.querySelector('.media-grid');
    var viewButtons = document.querySelectorAll('[data-media-view]');
    if (!grid || !viewButtons.length) return;

    viewButtons.forEach(function (btn) {
      btn.addEventListener('click', function () {
        var view = btn.getAttribute('data-media-view');
        grid.classList.toggle('media-list-view', view === 'list');
        viewButtons.forEach(function (b) { b.classList.toggle('active', b === btn); });
      });
    });

    // Chọn ảnh (đánh dấu is-selected)
    grid.querySelectorAll('.media-item').forEach(function (item) {
      item.addEventListener('click', function (e) {
        if (e.target.closest('button, a')) return;
        item.classList.toggle('is-selected');
      });
    });
  }

  /* ---------------------------------------------------------------
   * Copy URL ảnh trong media library
   * --------------------------------------------------------------- */
  function initCopyUrl() {
    document.querySelectorAll('[data-copy]').forEach(function (btn) {
      btn.addEventListener('click', function (e) {
        e.preventDefault();
        var text = btn.getAttribute('data-copy');
        if (navigator.clipboard) {
          navigator.clipboard.writeText(text).then(function () {
            var old = btn.innerHTML;
            btn.innerHTML = '<i class="bi bi-check-lg"></i>';
            window.setTimeout(function () { btn.innerHTML = old; }, 900);
          });
        }
      });
    });
  }

  /* ---------------------------------------------------------------
   * Upload drop: hiện preview ảnh + tên tệp sau khi chọn
   * --------------------------------------------------------------- */
  function initUploadPreview() {
    document.querySelectorAll('.upload-drop input[type="file"]').forEach(function (input) {
      var drop = input.closest('.upload-drop');
      if (!drop || drop.querySelector('.upload-preview')) return;

      var preview = document.createElement('div');
      preview.className = 'upload-preview mt-2';
      preview.hidden = true;
      drop.appendChild(preview);

      input.addEventListener('change', function () {
        var file = input.files && input.files[0];
        if (!file) {
          preview.hidden = true;
          preview.innerHTML = '';
          return;
        }
        var sizeMb = (file.size / 1024 / 1024).toFixed(2);
        preview.innerHTML =
          '<span class="small text-muted d-block text-truncate"><i class="bi bi-image" aria-hidden="true"></i> ' +
          file.name + ' (' + sizeMb + ' MB)</span>';
        preview.hidden = false;
        if (typeof FileReader !== 'undefined' && /^image\//.test(file.type)) {
          var reader = new FileReader();
          reader.onload = function (e) {
            var img = document.createElement('img');
            img.src = e.target.result;
            img.alt = 'Xem trước ảnh đã chọn';
            img.width = 96;
            img.height = 72;
            img.style.objectFit = 'cover';
            img.className = 'rounded border';
            img.loading = 'lazy';
            preview.insertBefore(img, preview.firstChild);
          };
          reader.readAsDataURL(file);
        }
      });
    });
  }

  /* ---------------------------------------------------------------
   * Xóa dòng trong bảng con (phân loại giá, gallery...) — data-row-remove
   * --------------------------------------------------------------- */
  function initRowRemove() {
    document.addEventListener('click', function (e) {
      var btn = e.target.closest('[data-row-remove]');
      if (!btn) return;
      var row = btn.closest('tr');
      if (row) row.remove();
    });
  }

  /* ---------------------------------------------------------------
   * Form tự submit khi đổi control (switch bật/tắt...)
   * --------------------------------------------------------------- */
  function initAutoSubmit() {
    document.querySelectorAll('form[data-autosubmit]').forEach(function (form) {
      form.querySelectorAll('input, select').forEach(function (control) {
        control.addEventListener('change', function () {
          form.submit();
        });
      });
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initSidebar();
    initTopbarSearch();
    initSidebarSub();
    initCheckAll();
    initMediaToggle();
    initCopyUrl();
    initUploadPreview();
    initRowRemove();
    initAutoSubmit();
  });
})();
