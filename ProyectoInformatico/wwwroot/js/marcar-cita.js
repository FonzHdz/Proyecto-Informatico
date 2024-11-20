document.addEventListener("DOMContentLoaded", () => {
    const cancelarCitaBtns = document.querySelectorAll(".cancelarCitaBtn");
    const successMessage = document.getElementById('successMessage');
    const successText = document.getElementById('successText');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    cancelarCitaBtns.forEach((btn) => {
        btn.addEventListener("click", async () => {
            const citaId = btn.getAttribute("data-cita-id");
            try {
                const response = await fetch(`/citas/${citaId}/cancelar`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });

                if (response.ok) {
                    console.log("Cita ID:", citaId);
                    successText.textContent = 'Cita marcada como cancelada.';
                    successMessage.classList.add('show');

                    setTimeout(() => {
                        successMessage.classList.remove('show');
                        location.reload();
                    }, 3000);
                } else {
                    const errorData = await response.json();
                    errorText.textContent = errorData.mensaje || 'No se pudo cancelar la cita.';
                    errorMessage.classList.add('show');

                    setTimeout(() => {
                        errorMessage.classList.remove('show');
                    }, 3000);
                }
            } catch (error) {
                console.error("Error al cancelar la cita:", error);
                errorText.textContent = 'Ocurrió un error inesperado.';
                errorMessage.classList.add('show');

                setTimeout(() => {
                    errorMessage.classList.remove('show');
                }, 3000);
            }
        });
    });
});