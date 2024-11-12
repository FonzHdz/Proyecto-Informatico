document.addEventListener('DOMContentLoaded', function () {
    const registroForm = document.getElementById('registroForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    registroForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const contraseña = document.getElementById('password').value.trim();
        const confirmarContraseña = document.getElementById('confirm-password').value.trim();

        if (contraseña !== confirmarContraseña) {
            errorText.textContent = 'Las contraseñas no coinciden. Por favor, verifique e intente nuevamente.';
            errorMessage.classList.add('show');

            setTimeout(() => {
                errorMessage.classList.remove('show');
            }, 3000);
            return;
        }

        const formData = new FormData();
        formData.append('nombre', `${document.getElementById('nombre').value.trim()} ${document.getElementById('apellido').value.trim()}`);
        formData.append('cedula', document.getElementById('cedula').value.trim());
        formData.append('genero', document.getElementById('genero').value.trim());
        formData.append('fechaNacimiento', document.getElementById('fecha_nacimiento').value.trim());
        formData.append('telefono', document.getElementById('telefono').value.trim());
        formData.append('direccion', document.getElementById('direccion').value.trim());
        formData.append('departamento', document.getElementById('departamento').value.trim());
        formData.append('ciudad', document.getElementById('ciudad').value.trim());
        formData.append('especialidad', document.getElementById('especialidad').value.trim());
        formData.append('contraseña', contraseña);

        console.log("FormData enviado:", Array.from(formData.entries()));

        try {
            const response = await fetch('/registro-doctor', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                successMessage.classList.add('show');

                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 4000);

                setTimeout(() => {
                    window.location.href = '/acceso-doctor';
                }, 4100);
            } else {
                const data = await response.json();
                errorText.textContent = data.mensaje || 'Error en el registro.';
                errorMessage.classList.add('show');

                setTimeout(() => {
                    errorMessage.classList.remove('show');
                }, 3000);
            }
        } catch (error) {
            errorText.textContent = 'Ocurrió un error en el servidor. Inténtelo nuevamente.';
            errorMessage.classList.add('show');

            setTimeout(() => {
                errorMessage.classList.remove('show');
            }, 3000);
        }
    });
});