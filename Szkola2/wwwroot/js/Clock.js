function updateClock() {
    const now = new Date();
    const hours = now.getHours();
    const minutes = now.getMinutes();
    const seconds = now.getSeconds();

    const hourHand = document.querySelector('.hour-hand');
    const minuteHand = document.querySelector('.minute-hand');
    const secondHand = document.querySelector('.second-hand');

    const hourRotation = (hours % 12) * 30 + minutes / 2; // 360 degrees / 12 hours = 30 degrees per hour
    const minuteRotation = minutes * 6; // 360 degrees / 60 minutes = 6 degrees per minute
    const secondRotation = seconds * 6; // 360 degrees / 60 seconds = 6 degrees per second

    hourHand.style.transform = `rotate(${hourRotation}deg)`;
    minuteHand.style.transform = `rotate(${minuteRotation}deg)`;
    secondHand.style.transform = `rotate(${secondRotation}deg)`;
}
setInterval(updateClock, 1000);

function updateClocks() {
    const now = new Date();
    const hours = now.getHours();
    const minutes = now.getMinutes();
    const seconds = now.getSeconds();
    const day = now.getDate();
    const month = now.getMonth() + 1; // Dodaj 1, ponieważ styczeń zaczyna się od 0
    const year = now.getFullYear();

    const formattedTime = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    const formattedDate = `${day.toString().padStart(2, '0')}/${month.toString().padStart(2, '0')}/${year}`;

    const dateTimeString = `Aktualna data i godzina: ${formattedDate} ${formattedTime}`;

    document.getElementById('current-date-time').innerText = dateTimeString;
}

setInterval(updateClocks, 1000);