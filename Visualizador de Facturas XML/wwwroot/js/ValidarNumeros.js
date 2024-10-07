    document.addEventListener('DOMContentLoaded', function () {
            const form = document.querySelector('#formPost'); // Selecciona el formulario
    const inputPerfilID = document.querySelector('#PerfilID'); // Selecciona el campo de texto

    form.addEventListener('submit', function (event) {
                // Verifica si el valor del campo no es un número
                if (isNaN(inputPerfilID.value) || inputPerfilID.value.trim() === "") {
        // Limpiar el campo de texto
        inputPerfilID.value = '';
    // Muestra un mensaje de advertencia
    alert('Por favor, ingrese un ID de perfil válido (números solamente).');
    event.preventDefault(); // Prevenir el envío del formulario
                }
            });
        });
