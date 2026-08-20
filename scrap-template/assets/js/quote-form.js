/* =============================================================================
 * QUOTE-FORM.JS — QuickQuoteForm: validate UI, upload preview, các trạng thái
 * loading/success/error. Template-only: submit demo, backend wire thay sau.
 * ===========================================================================*/
(function () {
  'use strict';

  var PHONE_RE = /^(0|\+84)(\s|\.)?((3[2-9])|(5[689])|(7[06-9])|(8[1-689])|(9[0-46-9]))\d{7}$/;

  /* ---------------------------------------------------------------
   * Validate từng field, trả về true/false + đánh dấu is-invalid
   * --------------------------------------------------------------- */
  function validateField(field) {
    var wrap = field.closest('.form-field') || field.parentElement;
    if (!wrap) return true;

    var value = (field.value || '').trim();
    var ok = true;

    if (field.hasAttribute('required') && !value) ok = false;
    if (ok && field.type === 'tel' && value && !PHONE_RE.test(value.replace(/\s/g, ''))) ok = false;

    field.classList.toggle('is-invalid', !ok);
    var fb = wrap.querySelector('.invalid-feedback');
    if (fb) fb.style.display = ok ? 'none' : 'block';
    return ok;
  }

  /* ---------------------------------------------------------------
   * Upload preview — chọn nhiều ảnh, preview, gỡ bỏ
   * --------------------------------------------------------------- */
  function initUpload(zone) {
    var input = zone.querySelector('input[type="file"]');
    var previews = zone.parentElement.querySelector('.upload-previews');
    if (!input || !previews) return;

    var renderFiles = function (files) {
      Array.prototype.slice.call(files).forEach(function (file) {
        if (!file.type.match(/^image\//)) return;
        var reader = new FileReader();
        reader.onload = function (e) {
          var thumb = document.createElement('div');
          thumb.className = 'upload-thumb';
          thumb.innerHTML =
            '<img src="' + e.target.result + '" alt="Ảnh phế liệu đã chọn">' +
            '<button type="button" class="upload-remove" aria-label="Gỡ ảnh này">&times;</button>';
          thumb.querySelector('.upload-remove').addEventListener('click', function () {
            thumb.remove();
          });
          previews.appendChild(thumb);
        };
        reader.readAsDataURL(file);
      });
    };

    input.addEventListener('change', function () {
      renderFiles(input.files);
    });

    // Kéo & thả (desktop)
    ['dragenter', 'dragover'].forEach(function (evt) {
      zone.addEventListener(evt, function (e) {
        e.preventDefault();
        zone.classList.add('dragover');
      });
    });
    ['dragleave', 'drop'].forEach(function (evt) {
      zone.addEventListener(evt, function (e) {
        e.preventDefault();
        zone.classList.remove('dragover');
      });
    });
    zone.addEventListener('drop', function (e) {
      renderFiles(e.dataTransfer.files);
    });
  }

  /* ---------------------------------------------------------------
   * Khởi tạo mọi form .js-quote-form trong trang (kể cả trong modal)
   * --------------------------------------------------------------- */
  function initQuoteForms() {
    var forms = document.querySelectorAll('.js-quote-form');
    if (!forms.length) return;

    forms.forEach(function (form) {
      // upload zone trong form
      form.querySelectorAll('.upload-drop').forEach(initUpload);

      // validate khi rời field
      form.querySelectorAll('[required], input[type="tel"]').forEach(function (field) {
        field.addEventListener('blur', function () { validateField(field); });
        field.addEventListener('input', function () {
          if (field.classList.contains('is-invalid')) validateField(field);
        });
      });

      form.addEventListener('submit', function (e) {
        e.preventDefault();

        var allValid = true;
        form.querySelectorAll('[required], input[type="tel"]').forEach(function (field) {
          if (!validateField(field)) allValid = false;
        });
        if (!allValid) {
          var firstInvalid = form.querySelector('.is-invalid');
          if (firstInvalid) firstInvalid.focus();
          return;
        }

        // Demo state machine — backend thay bằng AJAX thật
        var body = form.querySelector('.quote-form-body');
        var state = form.querySelector('.quote-state');
        if (!body || !state) return;

        body.style.display = 'none';
        state.className = 'quote-state is-loading';
        state.innerHTML =
          '<div class="spinner-square" role="status" aria-label="Đang gửi"></div>' +
          '<p class="mb-0">Đang gửi thông tin…</p>';

        window.setTimeout(function () {
          state.className = 'quote-state is-success';
          state.innerHTML =
            '<i class="bi bi-patch-check-fill" aria-hidden="true"></i>' +
            '<h3>Đã nhận yêu cầu</h3>' +
            '<p class="text-muted-2 mb-0">Chúng tôi sẽ gọi lại báo giá trong [30 phút]. Vui lòng để ý điện thoại.</p>';
        }, 1200);
      });
    });
  }

  /* ---------------------------------------------------------------
   * Nút mở modal báo giá (data-quote-open) — cuộn tới form hoặc mở modal
   * --------------------------------------------------------------- */
  function initQuoteOpen() {
    document.querySelectorAll('[data-quote-open]').forEach(function (btn) {
      btn.addEventListener('click', function (e) {
        var modalEl = document.getElementById('quoteModal');
        if (modalEl) {
          e.preventDefault();
          bootstrap.Modal.getOrCreateInstance(modalEl).show();
        }
      });
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initQuoteForms();
    initQuoteOpen();
  });
})();
