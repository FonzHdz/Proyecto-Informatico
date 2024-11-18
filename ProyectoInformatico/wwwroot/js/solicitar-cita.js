document.addEventListener("DOMContentLoaded", function () {
    const citaForm = document.getElementById("citaForm");
    const successMessage = document.getElementById("successMessage");
    const successText = document.getElementById("successText");
    const errorMessage = document.getElementById("errorMessage");
    const errorText = document.getElementById("errorText");

    citaForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const formData = new FormData(citaForm);
        const fecha = formData.get("fecha");
        const hora = formData.get("hora");
        const especialista = formData.get("especialista");

        if (!fecha || !hora || !especialista) {
            errorText.textContent = "Todos los campos son obligatorios.";
            errorMessage.classList.add("show");
            setTimeout(() => errorMessage.classList.remove("show"), 3000);
            return;
        }

        try {
            const response = await fetch("/citas/crear-cita", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    fechaCita: `${fecha}T${hora}`,
                    especialista,
                }),
            });

            if (response.ok) {
                const result = await response.json();
                successText.textContent = result.mensaje || "Cita creada exitosamente.";
                successMessage.classList.add("show");
                setTimeout(() => {
                    successMessage.classList.remove("show");
                    window.location.href = document.referrer;
                }, 3000);
            } else {
                const errorData = await response.json();
                errorText.textContent = errorData.mensaje || "Error al crear la cita.";
                errorMessage.classList.add("show");
                setTimeout(() => errorMessage.classList.remove("show"), 3000);
            }
        } catch (error) {
            console.error("Error al solicitar la cita:", error);
            errorText.textContent = "Ocurrió un error inesperado. Intente nuevamente.";
            errorMessage.classList.add("show");
            setTimeout(() => errorMessage.classList.remove("show"), 3000);
        }
    });
});