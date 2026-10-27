function showtip(element, text) {
    var tooltip = document.getElementById('tooltip');
    tooltip.innerText = text;
    tooltip.classList.add('show');

    var rect = element.getBoundingClientRect();
    var mouseX = rect.left + window.scrollX;
    var mouseY = rect.top + window.scrollY;

    var offsetX = 45;
    var offsetY = 45;

    tooltip.style.left = (mouseX + offsetX) + 'px';
    tooltip.style.top = (mouseY + offsetY) + 'px';
}

function hidetip() {
    var tooltip = document.getElementById('tooltip');
    tooltip.classList.remove('show');
}