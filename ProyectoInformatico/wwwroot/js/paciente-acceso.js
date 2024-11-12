document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('patientLoginForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    loginForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const cedula = document.getElementById('cedula').value.trim();
        const password = document.getElementById('password').value.trim();

        const formData = new FormData();
        formData.append('cedula', cedula);
        formData.append('password', password);

        try {
            const response = await fetch('/acceso-paciente', {
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
                    window.location.href = `/panel-paciente?cedula=${data.pacienteId}`;
                }, 4100);
            } else {
                const data = await response.json();
                errorText.textContent = data.mensaje || 'Usuario o contraseña incorrectos.';
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