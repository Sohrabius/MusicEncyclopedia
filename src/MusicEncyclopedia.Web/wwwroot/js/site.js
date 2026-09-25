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
        // Prefer the <html lang> attribute set by CultureMiddleware,
        // fall back to the first URL segment for safety.
        var lang = document.documentElement ? document.documentElement.lang : '';
        if (lang && /^[a-z]{2}(-[A-Z]{2})?$/.test(lang)) {
            return lang.substring(0, 2).toLowerCase();
        }
        var m = window.location.pathname.match(/^\/([a-z]{2})\b/);
        return m ? m[1] : 'fa';
    }

    function localizeUrl(culture) {
        var path = window.location.pathname;
        var current = getCurrentCulture();
        var newPath = path.replace(new RegExp('^\\/' + current + '\\b'), '/' + culture);
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
        btn.setAttribute('aria-label', 'بازگشت به بالای صفحه');
        btn.className = 'no-print';
        btn.style.cssText = 'position:fixed;bottom:1.5rem;inset-inline-end:1.5rem;width:44px;height:44px;border-radius:8px;background:var(--color-ink);color:var(--color-accent-ink);border:none;cursor:pointer;display:flex;align-items:center;justify-content:center;box-shadow:0 2px 8px oklch(20% 0.01 60 / 0.1);opacity:0;visibility:hidden;transition:opacity 220ms ease,visibility 220ms ease;z-index:50;';
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
            var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            window.scrollTo({ top: 0, behavior: reduceMotion ? 'auto' : 'smooth' });
        });
    }

    // -----------------------------------------------------------------------
    // Mobile nav
    // -----------------------------------------------------------------------

    function setMobileNav(open) {
        var mobile = document.querySelector('.nav__mobile');
        var toggle = document.querySelector('.nav__toggle');
        if (mobile) {
            mobile.classList.toggle('is-open', open);
            mobile.setAttribute('aria-hidden', open ? 'false' : 'true');
            mobile.inert = !open;
        }
        if (toggle) {
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        }
    }

    function initMobileNav() {
        var toggle = document.querySelector('.nav__toggle');
        var mobile = document.querySelector('.nav__mobile');
        if (!toggle || !mobile) return;

        toggle.addEventListener('click', function () {
            var open = !mobile.classList.contains('is-open');
            setMobileNav(open);
            if (open) {
                var firstControl = mobile.querySelector('button, a');
                if (firstControl) firstControl.focus();
            }
        });

        var close = document.querySelector('.nav__mobile-close');
        if (close) {
            close.addEventListener('click', function () {
                setMobileNav(false);
                toggle.focus();
            });
        }

        // Close on link click
        mobile.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                setMobileNav(false);
                toggle.focus();
            });
        });

        // Close on Esc / on backdrop click
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && mobile.classList.contains('is-open')) {
                setMobileNav(false);
                toggle.focus();
            }
            if (e.key === 'Tab' && mobile.classList.contains('is-open')) {
                var focusable = Array.from(mobile.querySelectorAll('button, a'));
                var first = focusable[0];
                var last = focusable[focusable.length - 1];
                if (e.shiftKey && document.activeElement === first) {
                    e.preventDefault();
                    last.focus();
                } else if (!e.shiftKey && document.activeElement === last) {
                    e.preventDefault();
                    first.focus();
                }
            }
        });

        mobile.addEventListener('click', function (e) {
            if (e.target === mobile) {
                setMobileNav(false);
                toggle.focus();
            }
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
