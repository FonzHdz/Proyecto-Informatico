document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('contactForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        fetch(form.action, {
            method: 'POST',
            body: new FormData(form),
            headers: {
                'Accept': 'application/json'
            }
        }).then(response => {
            if (response.ok) {
                successMessage.classList.add('show');
                form.reset();

                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 5000);
            } else {
                errorText.textContent = 'Ocurrió un error al enviar el mensaje. Inténtelo nuevamente.';
                errorMessage.classList.add('show');

                setTimeout(() => {
                    errorMessage.classList.remove('show');
                }, 3000);
            }

        }).catch(error => {
            errorText.textContent = 'Ocurrió un error al enviar el mensaje. Inténtelo nuevamente.';
            errorMessage.classList.add('show');

            setTimeout(() => {
                errorMessage.classList.remove('show');
            }, 3000);
        });
    });
});