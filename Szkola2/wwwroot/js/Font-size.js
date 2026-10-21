(function () {
    var KEY = 'siteFontScale', MIN = 80, MAX = 150, STEP = 10, DEF = 100;

    function read() {
        try {
            var v = parseInt(localStorage.getItem(KEY), 10);
            return isNaN(v) ? DEF : Math.min(MAX, Math.max(MIN, v));
        } catch (e) { return DEF; }
    }
    function apply(v) {
        document.documentElement.style.fontSize = v + '%';
        try { localStorage.setItem(KEY, String(v)); } catch (e) { }
        window.dispatchEvent(new Event('resize'));
    }
    window.increaseFontSize = function () { apply(Math.min(MAX, read() + STEP)); };
    window.decreaseFontSize = function () { apply(Math.max(MIN, read() - STEP)); };

    var saved = read();
    if (saved !== DEF) document.documentElement.style.fontSize = saved + '%';
})();