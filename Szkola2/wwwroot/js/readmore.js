/*
 * "Czytaj dalej" na wąskich ekranach.
 * Użycie: <div data-readmore="150"> ...długa treść HTML... </div>
 * Na ekranie <= 768 px treść jest ucinana po N słowach (domyślnie 150),
 * pod nią pojawia się przycisk "Czytaj dalej" / "Zwiń".
 * Działa też dla treści w rozwijanych panelach Bootstrapa (liczy po ich otwarciu).
 */
(function () {
    var DEFAULT_WORDS = 150;
    var MIN_HIDDEN_PX = 80; // nie ucinaj, jeśli ukrywane byłoby mniej niż tyle
    var mq = window.matchMedia('(max-width: 768px)');

    // Zwraca prostokąt ostatniego dozwolonego słowa albo null, gdy tekst jest krótszy.
    function limitWordRect(el, limit) {
        var walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT, null);
        var count = 0;
        var lastRect = null;
        var node;

        while ((node = walker.nextNode())) {
            var re = /\S+/g;
            var m;
            while ((m = re.exec(node.nodeValue)) !== null) {
                count++;
                if (count === limit) {
                    var range = document.createRange();
                    range.setStart(node, m.index);
                    range.setEnd(node, m.index + m[0].length);
                    lastRect = range.getBoundingClientRect();
                } else if (count === limit + 1) {
                    return lastRect; // istnieje jeszcze co najmniej jedno słowo
                }
            }
        }
        return null;
    }

    function clear(el) {
        el.classList.remove('readmore-clamped');
        el.style.maxHeight = '';
        if (el._rmBtn) {
            el._rmBtn.remove();
            el._rmBtn = null;
        }
        el._rmState = null;
    }

    function apply(el) {
        if (el._rmState && el._rmState.expanded) return; // użytkownik rozwinął - nie ruszaj

        clear(el);

        if (!mq.matches) return;
        if (el.getClientRects().length === 0) return; // ukryty (np. zwinięty panel)

        var limit = parseInt(el.getAttribute('data-readmore'), 10) || DEFAULT_WORDS;
        var rect = limitWordRect(el, limit);
        if (!rect || !rect.height) return;

        var maxH = Math.ceil(rect.bottom - el.getBoundingClientRect().top) + 6;
        if (el.scrollHeight - maxH < MIN_HIDDEN_PX) return;

        var state = { expanded: false, maxH: maxH };
        el._rmState = state;

        el.classList.add('readmore-clamped');
        el.style.maxHeight = maxH + 'px';

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'readmore-btn';
        btn.textContent = 'Czytaj dalej';
        btn.setAttribute('aria-expanded', 'false');

        btn.addEventListener('click', function () {
            state.expanded = !state.expanded;
            if (state.expanded) {
                el.classList.remove('readmore-clamped');
                el.style.maxHeight = '';
                btn.textContent = 'Zwiń';
            } else {
                el.classList.add('readmore-clamped');
                el.style.maxHeight = state.maxH + 'px';
                btn.textContent = 'Czytaj dalej';
            }
            btn.setAttribute('aria-expanded', String(state.expanded));
        });

        el.insertAdjacentElement('afterend', btn);
        el._rmBtn = btn;

        // Spóźnione ładowanie obrazków zmienia wysokości - policz ponownie
        el.querySelectorAll('img').forEach(function (img) {
            if (!img.complete && !img._rmBound) {
                img._rmBound = true;
                img.addEventListener('load', function () { apply(el); });
            }
        });
    }

    function applyAll(root) {
        (root || document).querySelectorAll('[data-readmore]').forEach(apply);
    }

    document.addEventListener('DOMContentLoaded', function () { applyAll(); });
    window.addEventListener('load', function () { applyAll(); });

    // Zmiana szerokości okna / obrót telefonu
    if (mq.addEventListener) {
        mq.addEventListener('change', function () { applyAll(); });
    } else if (mq.addListener) {
        mq.addListener(function () { applyAll(); });
    }

    // Treść w rozwijanych panelach (Bootstrap collapse/accordion)
    document.addEventListener('shown.bs.collapse', function (e) {
        applyAll(e.target);
    });
})();
