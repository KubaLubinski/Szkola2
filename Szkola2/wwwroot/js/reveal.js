(function () {
    var SELECTORS = [
        ".page-heading", ".panel-card",
        ".contact-info", ".contact-card",
        ".doc-card", ".photo-card",
        ".history-entry", ".event-card"
    ].join(", ");

    var SKIP = ".info-card, .news-card, .quick-card, .section-heading";

    function init() {
        if (!("IntersectionObserver" in window)) return;
        if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;

        var items = Array.prototype.filter.call(document.querySelectorAll(SELECTORS), function (el) {
            return !el.matches(SKIP) && !el.closest(SKIP);
        });
        if (!items.length) return;

        var perParent = new Map();
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                var el = entry.target;
                el.classList.add("animate-in");
                observer.unobserve(el);
                el.addEventListener("transitionend", function done(e) {
                    if (e.target !== el) return;
                    el.style.transitionDelay = "";
                    el.removeEventListener("transitionend", done);
                });
            });
        }, { threshold: 0, rootMargin: "0px 0px -6% 0px" });

        items.forEach(function (el) {
            var n = perParent.get(el.parentNode) || 0;
            perParent.set(el.parentNode, n + 1);
            if (n > 0) el.style.transitionDelay = Math.min(n, 6) * 70 + "ms";
            el.classList.add("reveal");
            observer.observe(el);
        });
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
    else init();
})();
