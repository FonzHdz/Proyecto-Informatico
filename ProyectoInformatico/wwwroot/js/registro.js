document.getElementById('formularioRegistro').addEventListener('submit', async (e) => {
    e.preventDefault();

    const doctor = {
        nombre: document.getElementById('nombre').value,
        email: document.getElementById('email').value,
        password: document.getElementById('password').value
    };

    try {
        const response = await fetch('/api/Doctor/registrar', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(doctor)
        });

        if (response.ok) {
            alert('Doctor registrado con éxito');
            // Redirigir o limpiar el formulario
        } else {
            const errorMsg = await response.text();
            alert('Error: ' + errorMsg);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Error en el servidor');
    }
});