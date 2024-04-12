    // Funkcja do zapisywania preferowanego rozmiaru czcionki w pamięci przeglądarki
    function saveFontSize(fontSize) {
        localStorage.setItem('preferredFontSize', fontSize);
    }

    // Funkcja do wczytywania preferowanego rozmiaru czcionki z pamięci przeglądarki
    function loadFontSize() {
        return localStorage.getItem('preferredFontSize') || '16'; // Domyślny rozmiar czcionki: 16px
    }

    // Funkcja do ustawiania rozmiaru czcionki dla wszystkich elementów na stronie
    function setFontSize(fontSize) {
        document.body.style.fontSize = fontSize + 'px';
    }

    // Funkcja do zwiększania rozmiaru czcionki
    function increaseFontSize() {
        var currentFontSize = parseInt(loadFontSize());
    var newFontSize = Math.min(currentFontSize + 2, 24); // Maksymalny rozmiar: 24px
    saveFontSize(newFontSize);
    setFontSize(newFontSize);
    }

    // Funkcja do zmniejszania rozmiaru czcionki
    function decreaseFontSize() {
        var currentFontSize = parseInt(loadFontSize());
    var newFontSize = Math.max(currentFontSize - 2, 10); // Minimalny rozmiar: 10px
    saveFontSize(newFontSize);
    setFontSize(newFontSize);
    }

    // Po załadowaniu strony, ustaw preferowany rozmiar czcionki
    document.addEventListener('DOMContentLoaded', function () {
        var preferredFontSize = loadFontSize();
    setFontSize(preferredFontSize);
    });
