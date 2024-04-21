document.addEventListener("DOMContentLoaded", function () {
    const categorySelect = document.querySelector('[name="CategoryId"]');
    const eventDateInput = document.querySelector('[name="EventDate"]').closest('.form-group');

    if (!['1', '2'].includes(categorySelect.value)) {
        eventDateInput.style.display = 'none';
    }

    function toggleEventDateVisibility() {
        if (['1', '2'].includes(categorySelect.value)) {
            eventDateInput.style.display = 'block'; 
        } else {
            eventDateInput.style.display = 'none'; 
            eventDateInput.value = '01.01.0001 00:00:00';
        }
    }

    categorySelect.addEventListener('change', toggleEventDateVisibility);
});