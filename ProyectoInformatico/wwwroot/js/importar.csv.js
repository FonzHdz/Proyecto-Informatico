document.addEventListener('DOMContentLoaded', function () {
    const importForm = document.getElementById('importExcelForm');

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
                const result = await response.json();
                alert(result.mensaje || "Datos importados exitosamente.");
            } else {
                const error = await response.json();
                alert(error.mensaje || "Error al importar los datos.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("Ocurrió un error inesperado.");
        }
    });
});