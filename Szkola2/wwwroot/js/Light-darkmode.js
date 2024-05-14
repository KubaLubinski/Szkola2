
function applyDarkMode(isDarkMode) {   
    if (isDarkMode) {        
        document.body.classList.add("dark-theme");
    } else {             
        document.body.classList.remove("dark-theme");
    }
}

    document.addEventListener("DOMContentLoaded", function () {   
        var isDarkMode = localStorage.getItem("darkMode") === "true";    
        applyDarkMode(isDarkMode);
    });


// Funkcja do przełączania trybu ciemnego motywu
function toggleDarkMode() {
    
    var isDarkMode = localStorage.getItem("darkMode") === "true";
    isDarkMode = !isDarkMode;
    localStorage.setItem("darkMode", isDarkMode);
    updateDarkModeIcon(isDarkMode);
    window.location.reload();
}

// Funkcja do zmiany ikony na podstawie trybu ciemnego
function updateDarkModeIcon(isDarkMode) {
    var ldmodeIcon = document.getElementById("ldmode");
    if (isDarkMode) {       
        ldmodeIcon.setAttribute
            ("class", "bi bi-moon-fill icon-size transparent-button");
    } else {  
        ldmodeIcon.setAttribute
            ("class", "bi bi-brightness-high  icon-size transparent-button");
    }
}

    var ldmode = document.getElementById("ldmode");
    ldmode.addEventListener("click", toggleDarkMode);
    updateDarkModeIcon(localStorage.getItem("darkMode") === "true");




