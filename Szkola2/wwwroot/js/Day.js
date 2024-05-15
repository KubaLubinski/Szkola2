function createCalendar(year, month) {
    const firstDay = new Date(year, month, 0).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();

    const tbody = document.querySelector('#calendar tbody');
    tbody.innerHTML = '';

    let date = 1;
    for (let i = 0; i < 6; i++) {
        const row = document.createElement('tr');
        for (let j = 0; j < 7; j++) {
            if (i === 0 && j < firstDay) {
                const cell = document.createElement('td');
                cell.classList.add('calendar_td');
                row.appendChild(cell);
            } else if (date > daysInMonth) {
                break;
            } else {
                const cell = document.createElement('td');
                cell.classList.add('calendar_td');
                cell.textContent = date;
                if (date === new Date().getDate() && month === new Date().getMonth() && year === new Date().getFullYear()) {
                    cell.classList.add('thick-border');
                }
                row.appendChild(cell);
                date++;
            }
        }
        tbody.appendChild(row);
    }
}

const now = new Date();
const currentYear = now.getFullYear();
const currentMonth = now.getMonth();
createCalendar(currentYear, currentMonth);