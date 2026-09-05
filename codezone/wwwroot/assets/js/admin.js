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
        if (!input.files || !input.files.length) {
          preview.hidden = true;
          preview.innerHTML = '';
          return;
        }

        preview.innerHTML = '';
        preview.hidden = false;

        if (input.multiple && input.files.length > 1) {
          var totalSize = 0;
          for (var i = 0; i < input.files.length; i++) totalSize += input.files[i].size;
          var totalMb = (totalSize / 1024 / 1024).toFixed(2);

          var summary = document.createElement('span');
          summary.className = 'small text-success fw-semibold d-block mb-2';
          summary.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i> Đã chọn ' + input.files.length + ' ảnh (' + totalMb + ' MB)';
          preview.appendChild(summary);

          var thumbWrap = document.createElement('div');
          thumbWrap.className = 'd-flex flex-wrap gap-2';
          preview.appendChild(thumbWrap);

          Array.from(input.files).slice(0, 10).forEach(function (f) {
            if (typeof FileReader !== 'undefined' && /^image\//.test(f.type)) {
              var reader = new FileReader();
              reader.onload = function (e) {
                var img = document.createElement('img');
                img.src = e.target.result;
                img.alt = f.name;
                img.title = f.name;
                img.width = 64;
                img.height = 48;
                img.style.objectFit = 'cover';
                img.className = 'rounded border';
                img.loading = 'lazy';
                thumbWrap.appendChild(img);
              };
              reader.readAsDataURL(f);
            }
          });
          if (input.files.length > 10) {
            var more = document.createElement('span');
            more.className = 'small text-muted align-self-center';
            more.textContent = '+' + (input.files.length - 10) + ' ảnh nữa';
            thumbWrap.appendChild(more);
          }
        } else {
          var file = input.files[0];
          var sizeMb = (file.size / 1024 / 1024).toFixed(2);
          preview.innerHTML =
            '<span class="small text-muted d-block text-truncate"><i class="bi bi-image" aria-hidden="true"></i> ' +
            file.name + ' (' + sizeMb + ' MB)</span>';
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
      if (row) {
        var tbody = row.closest('tbody');
        row.remove();
        if (tbody && tbody.children.length === 0) {
          var wrap = tbody.closest('.table-admin-wrap');
          if (wrap && wrap.id !== 'priceRowsTable') wrap.remove();
        }
      }
    });
  }

  /* ---------------------------------------------------------------
   * Tự động sinh Slug tiếng Việt chuẩn khi gõ Tiêu đề / Tên
   * --------------------------------------------------------------- */
  function toSlug(str) {
    if (!str) return '';
    return str
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[đĐ]/g, 'd')
      .replace(/[^a-z0-9\s-]/g, '')
      .trim()
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-');
  }

  function initAutoSlug() {
    var titleInputs = document.querySelectorAll('#articleTitle, #scrapName, #serviceTitle, #projectTitle, #locationName, input[name="Title"], input[name="Name"]');
    titleInputs.forEach(function (titleInput) {
      var form = titleInput.closest('form');
      if (!form) return;
      var slugInput = form.querySelector('#articleSlug, #scrapSlug, #serviceSlug, #projectSlug, #locationSlug, input[name="Slug"]');
      if (!slugInput) return;

      var userTouched = Boolean(slugInput.value && slugInput.value.trim());
      slugInput.addEventListener('input', function () {
        userTouched = Boolean(slugInput.value.trim());
      });

      titleInput.addEventListener('input', function () {
        if (!userTouched || !slugInput.value.trim()) {
          slugInput.value = toSlug(titleInput.value);
        }
      });
    });
  }

  /* ---------------------------------------------------------------
   * Chống double-submit khi bấm nút Lưu form
   * --------------------------------------------------------------- */
  function initPreventDoubleSubmit() {
    document.querySelectorAll('form').forEach(function (form) {
      form.addEventListener('submit', function (e) {
        if (form.dataset.submitting === 'true') {
          e.preventDefault();
          return;
        }
        var submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
        if (submitButtons.length > 0) {
          setTimeout(function () {
            form.dataset.submitting = 'true';
            submitButtons.forEach(function (btn) {
              btn.disabled = true;
            });
          }, 50);
        }
      });
    });
  }

  /* ---------------------------------------------------------------
   * Bắt lỗi tải ảnh media preview
   * --------------------------------------------------------------- */
  function initMediaErrorHandling() {
    document.querySelectorAll('.media-setting-preview img').forEach(function (img) {
      img.addEventListener('error', function () {
        img.classList.add('is-error');
      });
      img.addEventListener('load', function () {
        img.classList.remove('is-error');
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
    initAutoSlug();
    initPreventDoubleSubmit();
    initMediaErrorHandling();
  });
})();
