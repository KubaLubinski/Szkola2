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
    var ldmodeImage = document.getElementById("ldmode");
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

// Pobranie przycisku do przełączania trybu ciemnego motywu
var ldmode = document.getElementById("ldmode");

// Dodanie nasłuchiwania zdarzenia kliknięcia do przycisku
ldmode.addEventListener("click", toggleDarkMode);

// Dodanie instrukcji warunkowej if do określenia, który obrazek wyświetlić
if (localStorage.getItem("darkMode") === "true") {
    // Wyświetlenie obrazka trybu ciemnego
    document.getElementById("ldmode").src = "~/img/logo/moon.png";
} else {
    // Wyświetlenie obrazka trybu jasnego
    document.getElementById("ldmode").src = "~/img/logo/sun.png";
}

