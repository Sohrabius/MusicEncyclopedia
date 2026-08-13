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
        var label = document.getElementById('home-load-more-label');
        var pagination = document.getElementById('home-pagination');
        if (!grid || !countEl || !label) return;

        var culture = btn.getAttribute('data-culture') || getCulture();
        var pageSize = parseInt(btn.getAttribute('data-pagesize') || '20', 10);
        var cap = parseInt(btn.getAttribute('data-cap') || '100', 10);
        var total = parseInt(btn.getAttribute('data-total') || '0', 10);
        var busy = false;

        btn.addEventListener('click', function () {
            if (busy) return;

            var next = parseInt(btn.getAttribute('data-next-page') || '1', 10);
            var loaded = next * pageSize;

            // The lazy zone ends at the cap — switch to numbered pagination.
            if (loaded > cap) {
                btn.hidden = true;
                if (pagination) pagination.hidden = false;
                return;
            }

            busy = true;
            btn.disabled = true;
            btn.classList.add('is-loading');

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
                    grid.insertAdjacentHTML('beforeend', html);

                    var shown = Math.min(loaded, total);
                    btn.setAttribute('data-next-page', String(next + 1));
                    if (countEl.dataset.template) {
                        countEl.textContent = countEl.dataset.template
                            .replace('{0}', shown)
                            .replace('{1}', total);
                    }

                    // Nothing left to lazy-load → hand over to pagination.
                    if (shown >= total || loaded >= cap) {
                        btn.hidden = true;
                        if (pagination) pagination.hidden = false;
                    }
                })
                .catch(function () {
                    // Keep the button enabled so the visitor can retry.
                })
                .finally(function () {
                    busy = false;
                    btn.disabled = false;
                    btn.classList.remove('is-loading');
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
            if (!card) return;

            btn.disabled = true;
            card.classList.add('is-loading');

            fetch(btn.getAttribute('data-url'), {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(function (res) {
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    return res.text();
                })
                .then(function (html) {
                    card.outerHTML = html;
                })
                .catch(function () {
                    card.classList.remove('is-loading');
                    var innerBtn = card.querySelector('#home-poem-shuffle');
                    if (innerBtn) innerBtn.disabled = false;
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
