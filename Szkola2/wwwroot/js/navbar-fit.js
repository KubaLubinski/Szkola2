(function () {
    var BREAKPOINT = 576;
    var raf = 0;

    function overflows(nav) {
        var container = nav.querySelector('.container-fluid') || nav;
        var brand = nav.querySelector('.navbar-brand');
        var collapse = nav.querySelector('.navbar-collapse');
        if (!collapse) return false;

        var cRect = collapse.getBoundingClientRect();
        var boxRect = container.getBoundingClientRect();
        var innerRight = boxRect.right - parseFloat(getComputedStyle(container).paddingRight || 0);

        if (cRect.right > innerRight + 1) return true;

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

        nav.classList.remove('navbar-force-collapse');
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

    var observer = new MutationObserver(schedule);
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['style', 'class'] });
    if (document.body) {
        observer.observe(document.body, { attributes: true, attributeFilter: ['style', 'class'] });
    }

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
