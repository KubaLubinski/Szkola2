function saveFontSize(fontSize) {
    localStorage.setItem('preferredFontSize', fontSize);
}

function loadFontSize() {
    const saved = parseInt(
        localStorage.getItem('preferredFontSize'),
        10
    );

    return Number.isFinite(saved)
        ? Math.min(24, Math.max(12, saved))
        : 16;
}

function setFontSize(fontSize) {
    fontSize = Math.min(24, Math.max(12, fontSize));

    document.documentElement.style.fontSize = fontSize + 'px';
    document.documentElement.style.setProperty(
        '--base-font-size',
        fontSize + 'px'
    );

    saveFontSize(fontSize);
}

function increaseFontSize() {
    const currentSize = loadFontSize();
    setFontSize(currentSize + 2);
}

function decreaseFontSize() {
    const currentSize = loadFontSize();
    setFontSize(currentSize - 2);
}

document.addEventListener('DOMContentLoaded', function () {
    setFontSize(loadFontSize());
});
