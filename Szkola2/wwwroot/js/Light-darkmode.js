
(function () {
    function updateDarkModeIcon(isDarkMode) {
        const icon = document.getElementById("ldmode");

        if (!icon) return;

        icon.className = isDarkMode
            ? "bi bi-moon-fill icon-size"
            : "bi bi-brightness-high icon-size";

        const button = icon.closest("button");

        if (button) {
            button.setAttribute(
                "aria-pressed",
                String(isDarkMode)
            );

            button.setAttribute(
                "aria-label",
                isDarkMode
                    ? "Włącz jasny motyw"
                    : "Włącz ciemny motyw"
            );

            button.title = isDarkMode
                ? "Włącz jasny motyw"
                : "Włącz ciemny motyw";
        }
    }

    function applyDarkMode(isDarkMode) {
        document.body.classList.toggle(
            "dark-theme",
            isDarkMode
        );

        document.documentElement.classList.toggle(
            "dark-pre",
            isDarkMode
        );

        updateDarkModeIcon(isDarkMode);
    }

    function toggleDarkMode() {
        const isDarkMode =
            !document.body.classList.contains("dark-theme");

        localStorage.setItem(
            "darkMode",
            String(isDarkMode)
        );

        applyDarkMode(isDarkMode);
    }

    function initializeDarkMode() {
        const isDarkMode =
            localStorage.getItem("darkMode") === "true";

        applyDarkMode(isDarkMode);

        const icon = document.getElementById("ldmode");
        const button = icon?.closest("button");

        if (button && !button.dataset.darkModeInitialized) {
            button.addEventListener("click", toggleDarkMode);
            button.dataset.darkModeInitialized = "true";
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initializeDarkMode
        );
    } else {
        initializeDarkMode();
    }
})();

