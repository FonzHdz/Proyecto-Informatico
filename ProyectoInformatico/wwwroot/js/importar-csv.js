document.addEventListener('DOMContentLoaded', function () {
    const importForm = document.getElementById('importExcelForm');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');
    const errorText = document.getElementById('errorText');

    importForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const formData = new FormData(importForm);
        const collection = formData.get('collectionSelect');

        if (!collection) {
            alert("Por favor, seleccione una colección.");
            return;
        }

        try {
            const response = await fetch('/importar-csv', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                successMessage.classList.add('show');

                setTimeout(() => {
                    successMessage.classList.remove('show');
                }, 3000);
                const result = await response.json();
            } else {
                const data = await response.json();
                errorText.textContent = data.mensaje || 'Error al importar los datos.';
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