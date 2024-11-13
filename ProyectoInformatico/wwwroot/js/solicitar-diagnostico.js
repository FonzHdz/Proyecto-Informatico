function solicitarDiagnostico(diagnosticoId) {
    const successMessage = document.getElementById('successMessage');
    const successText = document.getElementById('successText');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    fetch(`/paciente/enviar-diagnostico`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            diagnosticoId: diagnosticoId
        })
    })
    .then(response => {
        if (response.ok) {
            return response.json();
        } else {
            return response.json().then(data => {
                throw new Error(data.mensaje || "No se pudo enviar el diagnóstico.");
            });
        }
    })
    .then(data => {
        successText.textContent = data.mensaje || "Diagnóstico enviado correctamente.";
        successMessage.classList.add('show');

        setTimeout(() => {
            successMessage.classList.remove('show');
        }, 3000);
    })
    .catch(error => {
        console.error("Error:", error);
        errorText.textContent = error.message || "No se pudo enviar el diagnóstico.";
        errorMessage.classList.add('show');

        setTimeout(() => {
            errorMessage.classList.remove('show');
        }, 3000);
    });
}