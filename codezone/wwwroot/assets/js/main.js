/* =============================================================================
 * MAIN.JS — hành vi chung của public website.
 * Tách module IIFE, không làm ô nhiễm global scope (chỉ expose window.ST nhỏ).
 * ===========================================================================*/
(function () {
  'use strict';

  var ST = (window.ST = window.ST || {});

  /* ---------------------------------------------------------------
   * Sticky header — thêm class .is-stuck khi cuộn xuống
   * --------------------------------------------------------------- */
  function initStickyHeader() {
    var header = document.querySelector('.site-header');
    if (!header) return;

    var ticking = false;
    var isStuck = header.classList.contains('is-stuck');

    var onScroll = function () {
      if (ticking) return;
      ticking = true;
      requestAnimationFrame(function () {
        var y = window.scrollY || window.pageYOffset || 0;
        if (!isStuck && y > 64) {
          isStuck = true;
          header.classList.add('is-stuck');
        } else if (isStuck && y < 24) {
          isStuck = false;
          header.classList.remove('is-stuck');
        }
        ticking = false;
      });
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
  }

  /* ---------------------------------------------------------------
   * Reveal on scroll — fade/slide nhẹ cho .reveal (IntersectionObserver)
   * --------------------------------------------------------------- */
  function initReveal() {
    var items = document.querySelectorAll('.reveal');
    if (!items.length) return;

    if (!('IntersectionObserver' in window)) {
      items.forEach(function (el) { el.classList.add('is-in'); });
      return;
    }

    var io = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            entry.target.classList.add('is-in');
            io.unobserve(entry.target);
          }
        });
      },
      { rootMargin: '0px 0px -8% 0px', threshold: 0.05 }
    );

    items.forEach(function (el) { io.observe(el); });
  }

  /* ---------------------------------------------------------------
   * Number counter — đếm số liệu trong .stats-band (placeholder [XX])
   * Chỉ chạy khi số thật (data-count) được backend inject.
   * --------------------------------------------------------------- */
  function initCounter() {
    var nums = document.querySelectorAll('[data-count]');
    if (!nums.length || !('IntersectionObserver' in window)) return;

    var animate = function (el) {
      var target = parseFloat(el.getAttribute('data-count'));
      if (isNaN(target)) return;
      var suffix = el.getAttribute('data-count-suffix') || '';
      var dur = 900;
      var start = null;

      var tick = function (ts) {
        if (!start) start = ts;
        var p = Math.min((ts - start) / dur, 1);
        var eased = 1 - Math.pow(1 - p, 3);
        el.textContent = Math.round(target * eased).toLocaleString('vi-VN') + suffix;
        if (p < 1) requestAnimationFrame(tick);
      };
      requestAnimationFrame(tick);
    };

    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          animate(entry.target);
          io.unobserve(entry.target);
        }
      });
    }, { threshold: 0.4 });

    nums.forEach(function (el) { io.observe(el); });
  }

  /* ---------------------------------------------------------------
   * Back to top
   * --------------------------------------------------------------- */
  function initBackTop() {
    var btn = document.querySelector('.back-top');
    if (!btn) return;

    var reduceMotion =
      window.matchMedia &&
      window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    var onScroll = function () {
      btn.classList.toggle('show', window.scrollY > 560);
    };
    window.addEventListener('scroll', onScroll, { passive: true });

    // Native scrollTo({behavior:'smooth'}) hay bị hủy giữa đường trên mobile,
    // còn html đang có CSS scroll-behavior:smooth (main.css) khiến MỖI lời gọi
    // scrollTo() lại sinh thêm một animation bị ngắt liên tục → trang "giụt"
    // tại chỗ. Giải pháp: tắt scroll-behavior trong lúc animate thủ công rAF.
    var rafId = 0;
    var cleanup = function () {
      if (rafId) cancelAnimationFrame(rafId);
      rafId = 0;
      document.documentElement.style.scrollBehavior = '';
    };
    var cancel = function () {
      if (rafId) cleanup();
    };
    ['touchstart', 'wheel'].forEach(function (ev) {
      window.addEventListener(ev, cancel, { passive: true });
    });

    var animateToTop = function () {
      cleanup();

      var startY = window.scrollY || window.pageYOffset || 0;
      if (startY <= 0) return;

      document.documentElement.style.scrollBehavior = 'auto';

      var duration = 450;
      var startTime = null;
      var easeOutCubic = function (t) { return 1 - Math.pow(1 - t, 3); };

      var tick = function (ts) {
        if (!rafId) return;
        if (!startTime) startTime = ts;
        var progress = Math.min((ts - startTime) / duration, 1);
        window.scrollTo(0, Math.round(startY * (1 - easeOutCubic(progress))));
        if (progress < 1) {
          rafId = requestAnimationFrame(tick);
        } else {
          cleanup();
        }
      };
      rafId = requestAnimationFrame(tick);
    };

    btn.addEventListener('click', function () {
      if (reduceMotion) {
        cleanup();
        window.scrollTo(0, 0);
        return;
      }
      animateToTop();
    });
  }

  /* ---------------------------------------------------------------
   * Anchor nav active state (prices page) — highlight theo section
   * --------------------------------------------------------------- */
  function initAnchorNav() {
    var nav = document.querySelector('.anchor-nav');
    if (!nav || !('IntersectionObserver' in window)) return;

    var links = nav.querySelectorAll('a[href^="#"]');
    var sections = [];
    links.forEach(function (link) {
      var sec = document.querySelector(link.getAttribute('href'));
      if (sec) sections.push({ link: link, sec: sec });
    });
    if (!sections.length) return;

    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          links.forEach(function (l) { l.classList.remove('active'); });
          var hit = sections.filter(function (s) { return s.sec === entry.target; })[0];
          if (hit) hit.link.classList.add('active');
        }
      });
    }, { rootMargin: '-30% 0px -60% 0px' });

    sections.forEach(function (s) { io.observe(s.sec); });
  }

  /* ---------------------------------------------------------------
   * Năm hiện tại ở footer
   * --------------------------------------------------------------- */
  function initYear() {
    var el = document.querySelector('[data-year]');
    if (el) el.textContent = new Date().getFullYear();
  }

  /* ---------------------------------------------------------------
   * Carousel nhóm phế liệu ở trang chủ (.cat-swiper)
   * 4 card/lượt trên desktop, vuốt hoặc bấm mũi tên.
   * --------------------------------------------------------------- */
  function initCatCarousel() {
    document.querySelectorAll('.cat-swiper').forEach(function (el) {
      if (typeof Swiper === 'undefined') return;
      new Swiper(el, {
        slidesPerView: 1.15,
        spaceBetween: 16,
        grabCursor: true,
        navigation: {
          nextEl: el.querySelector('.swiper-button-next'),
          prevEl: el.querySelector('.swiper-button-prev')
        },
        breakpoints: {
          576: { slidesPerView: 2, spaceBetween: 16 },
          992: { slidesPerView: 3, spaceBetween: 20 },
          1200: { slidesPerView: 4, spaceBetween: 20 }
        }
      });
    });
  }

  /* ---------------------------------------------------------------
   * Bootstrap: khởi tạo tooltip/toast nếu có trong trang (no-op nếu chưa dùng)
   * --------------------------------------------------------------- */

  /* ---------------------------------------------------------------
   * Custom select — design chung cho mọi combobox trong .cat-filter-bar.
   * Native select bị ẩn (CSS), JS tự dựng .custom-select đi kèm,
   * đồng bộ value + dispatch change để filter cũ vẫn chạy.
   * Fix chữ Việt bị che: line-height 1.5 + padding 2px 0 cho .cs-text.
   * --------------------------------------------------------------- */
  function initCustomSelects() {
    document.querySelectorAll('.cat-filter-bar select').forEach(function (select) {
      if (select.dataset.csEnhanced) return;
      // Nếu đã có custom đi kèm (markup thủ công) thì bỏ qua
      if (select.nextElementSibling && select.nextElementSibling.classList &&
          select.nextElementSibling.classList.contains('custom-select')) {
        select.dataset.csEnhanced = '1';
        select.style.display = 'none';
        return;
      }
      select.dataset.csEnhanced = '1';
      select.style.display = 'none';

      var wrap = document.createElement('div');
      wrap.className = 'custom-select';
      var trigger = document.createElement('button');
      trigger.type = 'button';
      trigger.className = 'cs-trigger';
      trigger.setAttribute('aria-haspopup', 'listbox');
      var label = document.createElement('span');
      label.className = 'cs-text';
      var caret = document.createElement('i');
      caret.className = 'bi bi-chevron-down';
      caret.setAttribute('aria-hidden', 'true');
      trigger.appendChild(label);
      trigger.appendChild(caret);
      var list = document.createElement('ul');
      list.className = 'cs-options';
      list.setAttribute('role', 'listbox');
      wrap.appendChild(trigger);
      wrap.appendChild(list);
      select.after(wrap);

      function syncLabel() {
        var opt = select.options[select.selectedIndex];
        var txt = opt ? opt.textContent : '';
        label.textContent = txt;
        trigger.setAttribute('title', txt);
        trigger.setAttribute('aria-label', txt);
        list.querySelectorAll('li').forEach(function (li) {
          li.classList.toggle('active', li.dataset.value === select.value);
        });
      }

      Array.from(select.options).forEach(function (opt) {
        var li = document.createElement('li');
        li.textContent = opt.textContent;
        li.setAttribute('title', opt.textContent);
        li.dataset.value = opt.value;
        li.setAttribute('role', 'option');
        li.addEventListener('click', function () {
          select.value = opt.value;
          syncLabel();
          wrap.classList.remove('open');
          select.dispatchEvent(new Event('change', { bubbles: true }));
        });
        list.appendChild(li);
      });
      syncLabel();

      trigger.addEventListener('click', function (e) {
        e.stopPropagation();
        document.querySelectorAll('.custom-select.open').forEach(function (o) {
          if (o !== wrap) o.classList.remove('open');
        });
        wrap.classList.toggle('open');
      });
      document.addEventListener('click', function (e) {
        if (!wrap.contains(e.target)) wrap.classList.remove('open');
      });
      select.addEventListener('cs:sync', syncLabel);
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initStickyHeader();
    initReveal();
    initCounter();
    initBackTop();
    initAnchorNav();
    initYear();
    initCatCarousel();
    initCustomSelects();
  });
})();
