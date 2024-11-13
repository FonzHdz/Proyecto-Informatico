document.addEventListener("DOMContentLoaded", () => {
    const actualizarPerfilBtn = document.getElementById("actualizarPerfilBtn");
    const successMessage = document.getElementById('successMessage');
    const successText = document.getElementById('successText');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    actualizarPerfilBtn.addEventListener("click", async () => {
        const form = document.getElementById("perfilForm");
        const formData = new FormData(form);

        const cedula = document.getElementById("cedula").value;
        formData.append("cedula", cedula);

        try {
            const response = await fetch("/actualizar-perfil", {
                method: "POST",
                body: formData,
            });

            if (response.ok) {
                successText.textContent = 'Perfil actualizado correctamente.';
                successMessage.classList.add('show');

                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 3000);

            } else {
                const data = await response.json();
                errorText.textContent = data.mensaje || 'Error al actualizar el perfil.';
                errorMessage.classList.add('show');

                setTimeout(() => {
                    errorMessage.classList.remove('show');
                }, 3000);
            }
        } catch (error) {
            console.error("Error:", error);
            errorText.textContent = error || 'Ocurrió un error inesperado.';
            errorMessage.classList.add('show');

            setTimeout(() => {
                errorMessage.classList.remove('show');
            }, 3000);
        }
    });
});