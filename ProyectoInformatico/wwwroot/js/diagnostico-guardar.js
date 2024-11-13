document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('diagnosticoForm');
    const guardarBtn = document.getElementById('guardarDiagnostico');
    const successMessage = document.getElementById('successMessage');
    const successText = document.getElementById('successText');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    if (!form || !guardarBtn) {
        console.error('Formulario o botón no encontrado.');
        return;
    }

    guardarBtn.removeEventListener('click', guardarDiagnostico);
    guardarBtn.addEventListener('click', async function (event) {
        event.preventDefault();
        const formData = new FormData(form);
        console.log([...formData.entries()])

        const videoEcografia = document.getElementById("videoEcografia").files[0];
        const imagenesRadiologicas = document.getElementById("imagenesRadiologicas").files;

        if (videoEcografia) {
            formData.append("videoEcografia", videoEcografia);
        }

        for (let i = 0; i < imagenesRadiologicas.length; i++) {
            formData.append("imagenesRadiologicas", imagenesRadiologicas[i]);
        }

        try {
            const response = await fetch('/diagnostico/guardar', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                const result = await response.json();
                successText.textContent = result.mensaje || 'Diagnóstico guardado correctamente.';
                successMessage.classList.add('show');

                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 3000);
            } else {
                const error = await response.json();
                errorText.textContent = error.mensaje || 'Error al guardar el diagnóstico.';
                errorMessage.classList.add('show');

                setTimeout(() => {
                    errorMessage.classList.remove('show');
                }, 3000);
            }
        } catch (err) {
            console.error('Error:', err);
            errorText.textContent = 'Ocurrió un error inesperado.';
            errorMessage.classList.add('show');

            setTimeout(() => {
                errorMessage.classList.remove('show');
            }, 3000);
        }
    });
});