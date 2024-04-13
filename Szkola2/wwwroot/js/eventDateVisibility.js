document.addEventListener("DOMContentLoaded", function () {
    // Get the select dropdown element by its name
    const categorySelect = document.querySelector('[name="CategoryId"]');
    const eventDateInput = document.querySelector('[name="EventDate"]').closest('.form-group');

    // Hide the EventDate input initially if the page loads without a valid category selected
    if (!['1', '2'].includes(categorySelect.value)) {
        eventDateInput.style.display = 'none';
    }

    // Define the function to toggle visibility based on the selected category
    function toggleEventDateVisibility() {
        if (['1', '2'].includes(categorySelect.value)) {
            eventDateInput.style.display = 'block'; // Show the EventDate input
        } else {
            eventDateInput.style.display = 'none'; // Hide the EventDate input
            eventDateInput.value = '01.01.0001 00:00:00';
        }
    }

    // Bind the toggle function to the change event of the select dropdown
    categorySelect.addEventListener('change', toggleEventDateVisibility);
});