   
    function saveFontSize(fontSize) {
        localStorage.setItem('preferredFontSize', fontSize);
    }

    
    function loadFontSize() {
        return localStorage.getItem('preferredFontSize') || '16'; 
    }

    function setFontSize(fontSize) {
        document.body.style.fontSize = fontSize + 'px';
    }

    function increaseFontSize() {
        var currentFontSize = parseInt(loadFontSize());
    var newFontSize = Math.min(currentFontSize + 2, 24);
    saveFontSize(newFontSize);
    setFontSize(newFontSize);
    }


    function decreaseFontSize() {
        var currentFontSize = parseInt(loadFontSize());
    var newFontSize = Math.max(currentFontSize - 2, 10); 
    saveFontSize(newFontSize);
    setFontSize(newFontSize);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var preferredFontSize = loadFontSize();
    setFontSize(preferredFontSize);
    });
