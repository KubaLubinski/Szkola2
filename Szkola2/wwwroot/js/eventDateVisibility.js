document.addEventListener("DOMContentLoaded", function () {
    const categorySelect = document.querySelector('[name="CategoryId"]');
    const eventDateInput = document.querySelector('[name="EventDate"]');

    // Skrypt jest współdzielony - na stronach bez tych pól po prostu nic nie robi
    if (!categorySelect || !eventDateInput) return;

    // Obsługuje zarówno nowy układ (.form-field), jak i stary (.form-group)
    const wrapper = eventDateInput.closest('.form-field, .form-group');
    if (!wrapper) return;

    const categoriesWithDate = ['1', '2'];

    function toggleEventDateVisibility() {
        wrapper.style.display = categoriesWithDate.includes(categorySelect.value) ? '' : 'none';
    }

    toggleEventDateVisibility();
    categorySelect.addEventListener('change', toggleEventDateVisibility);
});