document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('adminLoginForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    loginForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const usuario = document.getElementById('usuario').value.trim();
        const contraseña = document.getElementById('contraseña').value.trim();

        const formData = new FormData();
        formData.append('usuario', usuario);
        formData.append('contraseña', contraseña);
        successMessage.classList.remove('show');
        errorMessage.classList.remove('show');

        try {
            const response = await fetch('/admin-login', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                const data = await response.json();
                successMessage.classList.add('show');

                setTimeout(() => {
                    successMessage.classList.remove('show');
                    window.location.href = `/panel-admin?usuario=${usuario}`;
                }, 4000);
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