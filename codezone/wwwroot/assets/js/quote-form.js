/* =============================================================================
 * QUOTE-FORM.JS — QuickQuoteForm: validate UI, upload preview, các trạng thái
 * loading/success/error and AJAX submit.
 * ===========================================================================*/
(function () {
  'use strict';

  var PHONE_RE = /^(0|\+84)(\s|\.)?((3[2-9])|(5[689])|(7[06-9])|(8[1-689])|(9[0-46-9]))\d{7}$/;
  var MAX_IMAGES = 3;

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
    var form = zone.closest('form');
    var state = form ? form.querySelector('.quote-state') : null;
    if (!input || !previews) return;

    var showUploadError = function (message) {
      if (!state) return;
      state.className = 'quote-state is-error';
      state.innerHTML =
        '<i class="bi bi-exclamation-triangle-fill" aria-hidden="true"></i>' +
        '<p class="mb-0">' + message + '</p>';
    };

    var syncFiles = function (files, append) {
      var imageFiles = Array.prototype.slice.call(files || []).filter(function (file) {
        return file.type.match(/^image\//);
      });
      var currentFiles = append ? Array.prototype.slice.call(input.files || []) : [];
      var combined = currentFiles.concat(imageFiles);
      if (combined.length > MAX_IMAGES) {
        combined = combined.slice(0, MAX_IMAGES);
        showUploadError('Bạn chỉ gửi tối đa 3 ảnh.');
      }

      if (window.DataTransfer) {
        var transfer = new DataTransfer();
        combined.forEach(function (file) { transfer.items.add(file); });
        input.files = transfer.files;
      }

      return combined;
    };

    var setInputFiles = function (files) {
      if (!window.DataTransfer) {
        showUploadError('Trình duyệt không hỗ trợ kéo thả ảnh, vui lòng bấm chọn file.');
        return [];
      }
      return syncFiles(files, true);
    };

    var renderFiles = function (files) {
      previews.innerHTML = '';
      Array.prototype.slice.call(files || []).forEach(function (file) {
        if (!file.type.match(/^image\//)) return;
        var reader = new FileReader();
        reader.onload = function (e) {
          var thumb = document.createElement('div');
          thumb.className = 'upload-thumb';
          thumb.innerHTML =
            '<img src="' + e.target.result + '" alt="Ảnh phế liệu đã chọn">' +
            '<button type="button" class="upload-remove" aria-label="Gỡ ảnh này">&times;</button>';
          thumb.querySelector('.upload-remove').addEventListener('click', function () {
            var index = Array.prototype.indexOf.call(previews.children, thumb);
            if (window.DataTransfer && index >= 0) {
              var transfer = new DataTransfer();
              Array.prototype.slice.call(input.files || []).forEach(function (existingFile, fileIndex) {
                if (fileIndex !== index) transfer.items.add(existingFile);
              });
              input.files = transfer.files;
            }
            thumb.remove();
          });
          previews.appendChild(thumb);
        };
        reader.readAsDataURL(file);
      });
    };

    var replaceInputFiles = function (files) {
      var transfer = new DataTransfer();
      syncFiles(files, false).forEach(function (file) {
        transfer.items.add(file);
      });
      input.files = transfer.files;
    };

    input.addEventListener('change', function () {
      replaceInputFiles(input.files);
      renderFiles(input.files);
    });

    zone.addEventListener('click', function (e) {
      if (e.target === input) return;
      input.click();
    });

    zone.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        input.click();
      }
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
      renderFiles(setInputFiles(e.dataTransfer.files));
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

        var body = form.querySelector('.quote-form-body');
        var state = form.querySelector('.quote-state');
        if (!body || !state) return;

        body.style.display = 'none';
        state.className = 'quote-state is-loading';
        state.innerHTML =
          '<div class="spinner-square" role="status" aria-label="Đang gửi"></div>' +
          '<p class="mb-0">Đang gửi thông tin…</p>';

        var formData = new FormData(form);
        formData.set('SourceUrl', window.location.pathname + window.location.search);

        var token = form.querySelector('input[name="__RequestVerificationToken"]');

        fetch(form.action || '/contact/quick-quote', {
          method: 'POST',
          body: formData,
          headers: {
            'X-Requested-With': 'XMLHttpRequest',
            'RequestVerificationToken': token ? token.value : ''
          }
        })
          .then(function (response) {
            return response.text().then(function (text) {
              var data = {};
              if (text) {
                try {
                  data = JSON.parse(text);
                } catch (e) {
                  throw new Error('Máy chủ trả về dữ liệu không hợp lệ. Vui lòng tải lại trang rồi gửi lại.');
                }
              }
              if (!response.ok || !data.ok) throw new Error(data.message || 'Không gửi được yêu cầu.');
              return data;
            });
          })
          .then(function (data) {
            state.className = 'quote-state is-success';
            state.innerHTML =
              '<i class="bi bi-patch-check-fill" aria-hidden="true"></i>' +
              '<h3>Đã nhận yêu cầu</h3>' +
              '<p class="text-muted-2 mb-0">Mã yêu cầu ' + data.code + '. Chúng tôi sẽ gọi lại báo giá trong [30 phút].</p>';
            form.reset();
            form.querySelectorAll('.upload-previews').forEach(function (preview) { preview.innerHTML = ''; });
          })
          .catch(function (error) {
            body.style.display = '';
            state.className = 'quote-state is-error';
            state.innerHTML =
              '<i class="bi bi-exclamation-triangle-fill" aria-hidden="true"></i>' +
              '<p class="mb-0">' + (error.message || 'Không gửi được yêu cầu, vui lòng thử lại.') + '</p>';
          });
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
