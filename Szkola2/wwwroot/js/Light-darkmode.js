// Funkcja do obsługi przełączania trybu ciemnego motywu
function toggleDarkMode() {
    // Pobranie aktualnego trybu ciemnego motywu z lokalnego magazynu
    var isDarkMode = localStorage.getItem("darkMode") === "true";

    // Przełączanie trybu ciemnego motywu
    isDarkMode = !isDarkMode;

    // Zapisanie nowego trybu ciemnego motywu w lokalnym magazynie
    localStorage.setItem("darkMode", isDarkMode);

    // Wywołanie funkcji do zmiany wyglądu strony na podstawie aktualnego trybu
    applyDarkMode(isDarkMode);
}

// Funkcja do zmiany wyglądu strony na podstawie trybu ciemnego motywu
function applyDarkMode(isDarkMode) {
    
    if (isDarkMode) {
        // Dodanie klasy dark-theme do ciała dokumentu
        document.body.classList.add("dark-theme");
    } else {       
        // Usunięcie klasy dark-theme z ciała dokumentu
        document.body.classList.remove("dark-theme");
    }
}

    // Wczytanie ustawienia trybu ciemnego motywu przy załadowaniu strony
    document.addEventListener("DOMContentLoaded", function () {
    // Pobranie aktualnego trybu ciemnego motywu z lokalnego magazynu
        var isDarkMode = localStorage.getItem("darkMode") === "true";
    // Zastosowanie aktualnego trybu ciemnego motywu
        applyDarkMode(isDarkMode);
    });


// Funkcja do przełączania trybu ciemnego motywu
function toggleDarkMode() {
    // Zmiana trybu ciemnego
    var isDarkMode = localStorage.getItem("darkMode") === "true";
    isDarkMode = !isDarkMode;
    localStorage.setItem("darkMode", isDarkMode);

    // Aktualizacja ikony na podstawie trybu ciemnego
    updateDarkModeIcon(isDarkMode);

    // Odświeżenie strony
    window.location.reload();
}

// Funkcja do zmiany ikony na podstawie trybu ciemnego
function updateDarkModeIcon(isDarkMode) {
    var ldmodeIcon = document.getElementById("ldmode");

    if (isDarkMode) {
        // Ustawienie ikony trybu ciemnego (Bootstrap)
        ldmodeIcon.setAttribute("class", "bi bi-moon-fill text-warning transparent-button"); // Aktualizacja klasy ikony na tryb ciemny
    } else {
        // Ustawienie ikony trybu jasnego (Bootstrap)
        ldmodeIcon.setAttribute("class", "bi bi-brightness-high transparent-button"); // Aktualizacja klasy ikony na tryb jasny
    }
}

// Pobranie przycisku do przełączania trybu ciemnego motywu
    var ldmode = document.getElementById("ldmode");

// Dodanie nasłuchiwania zdarzenia kliknięcia do przycisku
    ldmode.addEventListener("click", toggleDarkMode);

// Wywołanie funkcji do aktualizacji ikony trybu na podstawie aktualnego trybu ciemnego
    updateDarkModeIcon(localStorage.getItem("darkMode") === "true");




