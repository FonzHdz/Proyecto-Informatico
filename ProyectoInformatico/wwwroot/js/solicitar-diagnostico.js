function solicitarDiagnostico(diagnosticoId) {
    fetch(`/paciente/enviar-diagnostico`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            diagnosticoId: diagnosticoId
        })
    })
    .then(response => response.json())
    .then(data => {
        alert(data.mensaje || "Diagnóstico enviado correctamente.");
    })
    .catch(error => {
        console.error("Error:", error);
        alert("No se pudo enviar el diagnóstico.");
    });
}