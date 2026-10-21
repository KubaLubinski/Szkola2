/*
 * Pasek nawigacji: gdy menu nie mieści się w jednej linii (np. po powiększeniu
 * tekstu), przełącza się na tryb zwijany (hamburger) - taki jak na wąskim ekranie.
 * Dodaje/usuwa klasę .navbar-force-collapse na #navbar (style w site.css).
 */
(function () {
    var BREAKPOINT = 576; // poniżej Bootstrap sam zwija pasek
    var raf = 0;

    function overflows(nav) {
        var container = nav.querySelector('.container-fluid') || nav;
        var brand = nav.querySelector('.navbar-brand');
        var collapse = nav.querySelector('.navbar-collapse');
        if (!collapse) return false;

        var cRect = collapse.getBoundingClientRect();
        var boxRect = container.getBoundingClientRect();
        var innerRight = boxRect.right - parseFloat(getComputedStyle(container).paddingRight || 0);

        // 1) zawartość wystaje poza pasek
        if (cRect.right > innerRight + 1) return true;

        // 2) zawinęła się pod logo (nie mieści się obok niego)
        if (brand) {
            var bRect = brand.getBoundingClientRect();
            if (cRect.top >= bRect.bottom - 1) return true;
        }
        return false;
    }

    function fit() {
        raf = 0;
        var nav = document.getElementById('navbar');
        if (!nav) return;

        nav.classList.remove('navbar-force-collapse'); // zmierz w układzie rozwiniętym
        if (window.innerWidth < BREAKPOINT) return;
        if (overflows(nav)) nav.classList.add('navbar-force-collapse');
    }

    function schedule() {
        if (!raf) raf = requestAnimationFrame(fit);
    }

    window.addEventListener('resize', schedule);
    window.addEventListener('load', schedule);
    document.addEventListener('DOMContentLoaded', schedule);
    if (document.fonts && document.fonts.ready) document.fonts.ready.then(schedule);

    // Zmiana wielkości tekstu / trybu kolorów zwykle zmienia styl lub klasę <html>/<body>
    var observer = new MutationObserver(schedule);
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['style', 'class'] });
    if (document.body) {
        observer.observe(document.body, { attributes: true, attributeFilter: ['style', 'class'] });
    }

    // Dodatkowo: po każdym kliknięciu przycisków zmiany rozmiaru tekstu
    ['increaseFontSize', 'decreaseFontSize'].forEach(function (name) {
        var original = window[name];
        if (typeof original === 'function') {
            window[name] = function () {
                var result = original.apply(this, arguments);
                schedule();
                setTimeout(schedule, 80);
                return result;
            };
        }
    });

    fit();
})();
