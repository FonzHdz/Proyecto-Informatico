document.addEventListener('DOMContentLoaded', function () {
    const registroForm = document.getElementById('registroPacienteForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    const esExtranjeroCheckbox = document.getElementById('esExtranjero');
    const nacionalidadSelect = document.getElementById('nacionalidad');

    const toggleNacionalidad = () => {
        if (esExtranjeroCheckbox.checked) {
            nacionalidadSelect.disabled = false;
        } else {
            nacionalidadSelect.disabled = true;
            nacionalidadSelect.value = 'Colombia';
        }
    };

    toggleNacionalidad();

    esExtranjeroCheckbox.addEventListener('change', toggleNacionalidad);

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
        formData.append('nombre', document.getElementById('nombre').value.trim());
        formData.append('fechaNacimiento', document.getElementById('fechaNacimiento').value.trim());
        formData.append('genero', document.getElementById('genero').value.trim());
        formData.append('correo', document.getElementById('correo').value.trim());
        formData.append('telefono', document.getElementById('telefono').value.trim());
        formData.append('direccion', document.getElementById('direccion').value.trim());
        formData.append('ciudad', document.getElementById('ciudad').value.trim());
        formData.append('departamento', document.getElementById('departamento').value.trim());
        formData.append('tipoSangre', document.getElementById('tipoSangre').value.trim());
        formData.append('semanasEmbarazo', document.getElementById('semanasEmbarazo').value.trim());
        formData.append('fechaUltimaEcografia', document.getElementById('fechaUltimaEcografia').value.trim());
        formData.append('alergias', document.getElementById('alergias').value.trim());
        formData.append('cedula', document.getElementById('cedula').value.trim());
        formData.append('contraseña', contraseña);
        formData.append('esExtranjero', document.getElementById('esExtranjero').checked ? 'true' : 'false');
        formData.append('nacionalidad', document.getElementById('nacionalidad').value.trim());
        formData.append('estadoCivil', document.getElementById('estadoCivil').value.trim());

        try {
            const response = await fetch('/registro-paciente', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                successMessage.classList.add('show');

                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 4000);

                setTimeout(() => {
                    window.location.href = '/acceso-paciente';
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