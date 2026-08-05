/**
 * Music Encyclopedia · site.js
 * Minimal interactions. No animation delays.
 */
(function () {
    'use strict';

    // -----------------------------------------------------------------------
    // Culture helpers
    // -----------------------------------------------------------------------

    function getCurrentCulture() {
        var m = window.location.pathname.match(/^\/(fa)\b/);
        return m ? m[1] : 'fa';
    }

    function localizeUrl(culture) {
        var path = window.location.pathname;
        var newPath = path.replace(/^\/(fa)\b/, '/' + culture);
        return newPath + window.location.search + window.location.hash;
    }

    // -----------------------------------------------------------------------
    // Debounce
    // -----------------------------------------------------------------------

    function debounce(fn, delay) {
        var timer;
        return function () {
            var args = arguments;
            var ctx = this;
            clearTimeout(timer);
            timer = setTimeout(function () { fn.apply(ctx, args); }, delay);
        };
    }

    // -----------------------------------------------------------------------
    // Search — ensure culture prefix on submit
    // -----------------------------------------------------------------------

    function initSearch() {
        var input = document.querySelector('.search__input');
        if (!input) return;
        var form = input.closest('form');
        if (!form) return;
        var culture = getCurrentCulture();

        form.addEventListener('submit', function (e) {
            if (!input.value.trim()) {
                e.preventDefault();
                input.focus();
                return;
            }
            var action = form.getAttribute('action') || '';
            if (!action.startsWith('/' + culture)) {
                form.action = '/' + culture + '/search';
            }
        });
    }

    // -----------------------------------------------------------------------
    // Back to top
    // -----------------------------------------------------------------------

    function initBackToTop() {
        var btn = document.createElement('button');
        btn.innerHTML = '<svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7"/></svg>';
        btn.setAttribute('aria-label', 'Back to top');
        btn.className = 'no-print';
        btn.style.cssText = 'position:fixed;bottom:1.5rem;inset-inline-end:1.5rem;width:36px;height:36px;border-radius:8px;background:var(--color-ink);color:var(--color-accent-ink);border:none;cursor:pointer;display:flex;align-items:center;justify-content:center;box-shadow:0 2px 8px oklch(20% 0.01 60 / 0.1);opacity:0;visibility:hidden;transition:opacity 220ms ease,visibility 220ms ease;z-index:50;';
        document.body.appendChild(btn);

        window.addEventListener('scroll', function () {
            if (window.scrollY > 400) {
                btn.style.opacity = '1';
                btn.style.visibility = 'visible';
            } else {
                btn.style.opacity = '0';
                btn.style.visibility = 'hidden';
            }
        });

        btn.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    // -----------------------------------------------------------------------
    // Mobile nav
    // -----------------------------------------------------------------------

    function initMobileNav() {
        var toggle = document.querySelector('.nav__toggle');
        var mobile = document.querySelector('.nav__mobile');
        var close = document.querySelector('.nav__mobile-close');
        if (!toggle || !mobile) return;

        toggle.addEventListener('click', function () {
            mobile.classList.add('is-open');
        });

        if (close) {
            close.addEventListener('click', function () {
                mobile.classList.remove('is-open');
            });
        }

        // Close on link click
        mobile.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                mobile.classList.remove('is-open');
            });
        });
    }

    // -----------------------------------------------------------------------
    // Init
    // -----------------------------------------------------------------------

    document.addEventListener('DOMContentLoaded', function () {
        initSearch();
        initBackToTop();
        initMobileNav();
    });

    // Expose
    window.MusicEncyclopedia = {
        getCurrentCulture: getCurrentCulture,
        localizeUrl: localizeUrl,
        debounce: debounce
    };

})();
