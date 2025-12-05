document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('reviewForm');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const text = document.getElementById('reviewText').value.trim();
        const rating = parseInt(document.getElementById('reviewRating').value || '5', 10);
        const msg = document.getElementById('reviewMessage');
        msg.innerText = '';

        try {
            const res = await fetch('/api/reviews', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ text, rating })
            });
            const data = await res.json();
            if (res.ok && data.success) {
                msg.innerHTML = '<div class="alert alert-success">Отзыв добавлен</div>';
                // prepend new review to the list
                window.location.href = '/Reviews';
            } else {
                msg.innerHTML = '<div class="alert alert-danger">Не удалось добавить отзыв</div>';
            }
        } catch (err) {
            msg.innerHTML = '<div class="alert alert-danger">Ошибка сети</div>';
        }
    });
});
