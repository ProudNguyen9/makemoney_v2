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

    var onScroll = function () {
      header.classList.toggle('is-stuck', window.scrollY > 40);
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

    var onScroll = function () {
      btn.classList.toggle('show', window.scrollY > 560);
    };
    window.addEventListener('scroll', onScroll, { passive: true });

    btn.addEventListener('click', function () {
      window.scrollTo({ top: 0, behavior: 'smooth' });
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
   * Bootstrap: khởi tạo tooltip/toast nếu có trong trang (no-op nếu chưa dùng)
   * --------------------------------------------------------------- */

  document.addEventListener('DOMContentLoaded', function () {
    initStickyHeader();
    initReveal();
    initCounter();
    initBackTop();
    initAnchorNav();
    initYear();
  });
})();
