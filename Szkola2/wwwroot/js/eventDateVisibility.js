document.addEventListener("DOMContentLoaded", function () {
    const categorySelect = document.querySelector('[name="CategoryId"]');
    const eventDateInput = document.querySelector('[name="EventDate"]');

    if (!categorySelect || !eventDateInput) return;

    const wrapper = eventDateInput.closest('.form-field, .form-group');
    if (!wrapper) return;

    const categoriesWithDate = ['1', '2'];

    function toggleEventDateVisibility() {
        wrapper.style.display = categoriesWithDate.includes(categorySelect.value) ? '' : 'none';
    }

    toggleEventDateVisibility();
    categorySelect.addEventListener('change', toggleEventDateVisibility);
});