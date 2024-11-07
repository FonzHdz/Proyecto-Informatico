async function fetchMessage() {
    try {
        const response = await fetch('/api/message');
        const data = await response.json();
        document.getElementById('message').innerText = data.message;
    } catch (error) {
        console.error('Error fetching message:', error);
    }
}