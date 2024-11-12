document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('doctorLoginForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    loginForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const usuario = document.getElementById('usuario').value.trim();
        const password = document.getElementById('password').value.trim();

        const formData = new FormData();
        formData.append('usuario', usuario);
        formData.append('password', password);

        try {
            const response = await fetch('/acceso-doctor', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                successMessage.classList.add('show');
                const data = await response.json();
                console.log(data.mensaje);

                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 4000);

                setTimeout(() => {
                    window.location.href = `/panel-doctor?id=${data.usuario}`;
                }, 4100);
            } else {
                errorText.textContent = 'Usuario o contraseña incorrectos.';
                errorMessage.classList.add('show');

                setTimeout(() => {
                    errorMessage.classList.remove('show');
                }, 3000);
            }
        } catch (error) {
            errorText.textContent = 'Ocurrió un error al iniciar sesión. Inténtelo nuevamente.';
            errorMessage.classList.add('show');

            setTimeout(() => {
                errorMessage.classList.remove('show');
            }, 3000);
        }
    });
});