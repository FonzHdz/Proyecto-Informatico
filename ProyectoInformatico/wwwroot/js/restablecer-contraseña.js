document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('resetPasswordForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');
    form.addEventListener('submit', async function (event) {
        event.preventDefault();

        const formData = new FormData(form);

        try {
            const response = await fetch('/restablecer-contraseña', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                const result = await response.json();
                successMessage.classList.add('show')

                setTimeout(() => {
                    successMessage.classList.remove('show');
                    window.location.href = document.referrer;
                }, 3000);
            } else {
                const error = await response.json();
                showMessage(errorMessage, error.mensaje || 'Error al actualizar el doctor.', 'error');
            }
        } catch (err) {
            console.error('Error:', err);
            errorText.textContent = err || 'Ocurrió un error inesperado.';
            errorMessage.classList.add('show')

            setTimeout(() => {
                errorMessage.classList.remove('show');
            }, 3000);
        }
    });
});