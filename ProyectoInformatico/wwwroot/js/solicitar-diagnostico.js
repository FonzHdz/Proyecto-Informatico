async function solicitarDiagnostico(diagnosticoId) {
    const successMessage = document.getElementById('successMessage');
    const successText = document.getElementById('successText');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    successMessage.classList.remove('show');
    errorMessage.classList.remove('show');

    successText.textContent = "Este proceso puede demorar unos segundos.";
    successMessage.classList.add('show');

    setTimeout(() => {
        successMessage.classList.remove('show');
    }, 8000);

    try {
        const response = await fetch(`/paciente/enviar-diagnostico`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ diagnosticoId: diagnosticoId }),
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.mensaje || "No se pudo enviar el diagnóstico.");
        }

        const data = await response.json();
        successText.textContent = data.mensaje || "Diagnóstico enviado correctamente.";
        successMessage.classList.add('show');
    } catch (error) {
        console.error("Error:", error);
        errorText.textContent = error.message || "No se pudo enviar el diagnóstico.";
        errorMessage.classList.add('show');
    } finally {
        setTimeout(() => {
            successMessage.classList.remove('show');
            errorMessage.classList.remove('show');
        }, 3000);
    }
}