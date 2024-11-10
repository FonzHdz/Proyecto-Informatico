document.addEventListener('DOMContentLoaded', function () {
    const dropbtn = document.querySelector('.dropbtn');
    const dropdownContent = document.querySelector('.dropdown-content');

    dropbtn.addEventListener('click', function (e) {
        e.preventDefault();
        dropdownContent.style.display = dropdownContent.style.display === 'block' ? 'none' : 'block';
    });

    // Cerrar el menú desplegable al hacer clic fuera de él
    document.addEventListener('click', function (e) {
        if (!e.target.matches('.dropbtn')) {
            dropdownContent.style.display = 'none';
        }
    });

    const form = document.getElementById('contactForm');
    const successMessage = document.getElementById('successMessage');

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        // Enviar el formulario usando Formspree
        fetch(form.action, {
            method: 'POST',
            body: new FormData(form),
            headers: {
                'Accept': 'application/json'
            }
        }).then(response => {
            if (response.ok) {
                // Mostrar el mensaje de éxito
                successMessage.classList.add('show');
                form.reset();

                // Ocultar el mensaje después de 5 segundos
                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 5000);
            } else {
                throw new Error('Error en el envío del formulario');
            }
        }).catch(error => {
            console.error('Error:', error);
            alert('Hubo un problema al enviar el mensaje. Por favor, inténtelo de nuevo.');
        });
    });

    // Función para el registro de doctores
    const registroForm = document.getElementById('registroForm');
    if (registroForm) {
        registroForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const nombre = document.getElementById('nombre').value;
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            registrarDoctor(nombre, email, password);
        });
    }

    // Función para el inicio de sesión (si lo necesitas en el futuro)
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            iniciarSesion(email, password);
        });
    }

    // Ejecutar las funciones de configuración
});

// Función para registrar un nuevo doctor
async function registrarDoctor(nombre, email, password) {
    try {
        const response = await fetch('/api/doctores/registro', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ nombre, email, password }),
        });

        const data = await response.json();
        if (response.ok) {
            alert('Registro exitoso.');
            // Puedes redirigir a otra página o limpiar el formulario aquí
        } else {
            alert(data.msg || 'Error en el registro');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Error en el registro: ' + error.message);
    }
}

// Función para iniciar sesión (si la necesitas en el futuro)
async function iniciarSesion(email, password) {
    // Implementa la lógica de inicio de sesión aquí si es necesario
}

async function verificarCodigoAutorizacion(codigo) {
    console.log('Iniciando verificación del código:', codigo);
    try {
        const response = await fetch('http://localhost:3000/api/doctores/verificar-codigo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ codigo }),
        });

        const data = await response.json();
        if (response.ok) {
            window.location.href = '/registro-doctor.html';
        } else {
            alert(data.msg || 'Código de autorización incorrecto');
        }
    } catch (error) {
        console.error('Error detallado:', error);
        alert('Error al verificar el código de autorización: ' + error.message);
    }
}

document.getElementById('autenticacionForm').addEventListener('submit', function (e) {
    e.preventDefault();
    const codigoAutorizacion = document.getElementById('codigoAutorizacion').value;
    console.log('Código ingresado:', codigoAutorizacion);
    verificarCodigoAutorizacion(codigoAutorizacion);
});