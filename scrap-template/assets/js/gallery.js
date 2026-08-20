/* =============================================================================
 * GALLERY.JS — khởi tạo Swiper cho gallery (project-detail, home nếu cần).
 * Chỉ nạp ở trang có .gallery-swiper; trang khác không dùng file này.
 * ===========================================================================*/
(function () {
  'use strict';

  function initGalleries() {
    var els = document.querySelectorAll('.gallery-swiper');
    if (!els.length || typeof Swiper === 'undefined') return;

    els.forEach(function (el) {
      new Swiper(el, {
        slidesPerView: 1,
        spaceBetween: 10,
        loop: false,
        navigation: {
          nextEl: el.querySelector('.swiper-button-next'),
          prevEl: el.querySelector('.swiper-button-prev')
        },
        pagination: {
          el: el.querySelector('.swiper-pagination'),
          clickable: true
        },
        keyboard: { enabled: true },
        breakpoints: {
          768: { slidesPerView: 2, spaceBetween: 12 },
          1200: { slidesPerView: 3, spaceBetween: 14 }
        }
      });
    });
  }

  document.addEventListener('DOMContentLoaded', initGalleries);
})();
