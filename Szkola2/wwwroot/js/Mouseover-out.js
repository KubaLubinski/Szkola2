function showtip(element, text) {
    var tooltip = document.getElementById('tooltip');
    tooltip.innerText = text;
    tooltip.classList.add('show');

    // Pobierz pozycję myszki
    var rect = element.getBoundingClientRect();
    var mouseX = rect.left + window.scrollX;
    var mouseY = rect.top + window.scrollY;

    // Ustaw przesunięcie w poziomie i pionie
    var offsetX = -240; // Przesunięcie w poziomie
    var offsetY = -140; // Przesunięcie w pionie

    // Ustaw pozycję dymku podpowiedzi
    tooltip.style.left = (mouseX + offsetX) + 'px';
    tooltip.style.top = (mouseY + offsetY) + 'px';
}

function hidetip() {
    var tooltip = document.getElementById('tooltip');
    tooltip.classList.remove('show');
}