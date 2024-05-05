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
       
        document.body.classList.add("dark-theme");
    } else {
    
        document.body.classList.remove("dark-theme");
    }
}

