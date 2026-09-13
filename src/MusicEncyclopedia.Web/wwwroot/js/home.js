/**
 * Music Encyclopedia · home.js
 * Lazy-loaded album grid ("Load more" up to 100, then numbered pagination)
 * and the random-poem sidebar shuffle button.
 */
(function () {
    'use strict';

    function getCulture() {
        var lang = document.documentElement ? document.documentElement.lang : '';
        if (lang && /^[a-z]{2}(-[A-Z]{2})?$/.test(lang)) {
            return lang.substring(0, 2).toLowerCase();
        }
        var m = window.location.pathname.match(/^\/([a-z]{2})\b/);
        return m ? m[1] : 'fa';
    }

    // -----------------------------------------------------------------------
    // "Load more" album grid
    // -----------------------------------------------------------------------

    function initLoadMore() {
        var btn = document.getElementById('home-load-more');
        if (!btn) return;

        var grid = document.getElementById('home-album-grid');
        var countEl = document.getElementById('home-loaded-count');
        var errorEl = document.getElementById('home-load-error');
        var label = document.getElementById('home-load-more-label');
        var pagination = document.getElementById('home-pagination');
        if (!grid || !countEl || !label) return;

        var culture = btn.getAttribute('data-culture') || getCulture();
        var pageSize = parseInt(btn.getAttribute('data-pagesize') || '20', 10);
        var cap = parseInt(btn.getAttribute('data-cap') || '100', 10);
        var total = parseInt(btn.getAttribute('data-total') || '0', 10);
        var shown = parseInt(btn.getAttribute('data-loaded') || '0', 10);
        var defaultLabel = label.textContent;
        var busy = false;

        btn.addEventListener('click', function () {
            if (busy) return;

            var next = parseInt(btn.getAttribute('data-next-page') || '1', 10);
            if (shown >= cap) {
                btn.hidden = true;
                if (pagination) pagination.hidden = false;
                return;
            }

            busy = true;
            btn.disabled = true;
            btn.setAttribute('aria-busy', 'true');
            btn.classList.add('is-loading');
            grid.setAttribute('aria-busy', 'true');
            label.textContent = btn.getAttribute('data-loading-message') || defaultLabel;
            if (errorEl) errorEl.hidden = true;

            var url = '/' + culture + '/home/albums?page=' + next;
            var category = btn.getAttribute('data-category');
            if (category) {
                url += '&category=' + encodeURIComponent(category);
            }

            fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(function (res) {
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    return res.text();
                })
                .then(function (html) {
                    var parsed = document.createElement('div');
                    parsed.innerHTML = html;
                    var added = parsed.querySelectorAll('.card--album').length;
                    if (added === 0) throw new Error('Empty album page');

                    grid.insertAdjacentHTML('beforeend', html);

                    shown = Math.min(shown + added, total, cap);
                    btn.setAttribute('data-next-page', String(next + 1));
                    btn.setAttribute('data-loaded', String(shown));
                    if (btn.getAttribute('data-loaded-template')) {
                        countEl.textContent = btn.getAttribute('data-loaded-template')
                            .replace('{0}', added)
                            .replace('{1}', shown)
                            .replace('{2}', total);
                    } else if (countEl.dataset.template) {
                        countEl.textContent = countEl.dataset.template
                            .replace('{0}', shown)
                            .replace('{1}', total);
                    }

                    // Nothing left to lazy-load → hand over to pagination.
                    if (shown >= total || shown >= cap) {
                        btn.hidden = true;
                        if (pagination && total > cap) pagination.hidden = false;
                    }
                })
                .catch(function () {
                    if (errorEl) errorEl.hidden = false;
                })
                .finally(function () {
                    busy = false;
                    btn.disabled = false;
                    btn.removeAttribute('aria-busy');
                    btn.classList.remove('is-loading');
                    grid.removeAttribute('aria-busy');
                    label.textContent = defaultLabel;
                });
        });
    }

    // -----------------------------------------------------------------------
    // Random poem shuffle (event delegation — the card is re-rendered)
    // -----------------------------------------------------------------------

    function initPoemShuffle() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest ? e.target.closest('#home-poem-shuffle') : null;
            if (!btn || btn.disabled) return;

            var card = document.getElementById('home-poem-card');
            var status = document.getElementById('home-poem-status');
            if (!card) return;

            btn.disabled = true;
            btn.setAttribute('aria-busy', 'true');
            card.classList.add('is-loading');
            if (status) {
                status.classList.remove('is-error');
                status.textContent = status.dataset.loading || '';
            }

            fetch(btn.getAttribute('data-url'), {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (res) {
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    if (res.status === 204) return '';
                    return res.text();
                })
                .then(function (html) {
                    if (html) {
                        card.outerHTML = html;
                        if (status) status.textContent = status.dataset.updated || '';
                    } else {
                        card.remove();
                        if (status) status.textContent = status.dataset.unavailable || '';
                    }
                })
                .catch(function () {
                    card.classList.remove('is-loading');
                    var innerBtn = card.querySelector('#home-poem-shuffle');
                    if (innerBtn) {
                        innerBtn.disabled = false;
                        innerBtn.removeAttribute('aria-busy');
                    }
                    if (status) {
                        status.classList.add('is-error');
                        status.textContent = status.dataset.error || '';
                    }
                });
        });
    }

    // -----------------------------------------------------------------------
    // Init
    // -----------------------------------------------------------------------

    document.addEventListener('DOMContentLoaded', function () {
        initLoadMore();
        initPoemShuffle();
    });

})();
