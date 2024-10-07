function openModal(url, title) {
    // Cambiar el título del modal
    document.getElementById('modalTitle').textContent = title;

    // Realizar la solicitud AJAX
    fetch(url)
        .then(response => response.text())
        .then(html => {
            // Colocar el contenido en el cuerpo del modal
            document.getElementById('modalBody').innerHTML = html;

            // Mostrar el modal solo si no está abierto ya
            var modalElement = document.getElementById('actionModal');
            var myModal = bootstrap.Modal.getInstance(modalElement); // Obtener instancia existente

            if (!myModal) {
                myModal = new bootstrap.Modal(modalElement, {
                    keyboard: false
                });
            }
            myModal.show();

            // Ajustar el tamaño del modal basado en el título
            if (title === "Detalles") {
                document.getElementById('ModalSize').classList.add('modal-xl');
            } else {
                document.getElementById('ModalSize').classList.remove('modal-xl');
            }
        })
        .catch(error => {
            console.error('Error al cargar el modal:', error);
        });

    // Eliminar y agregar el manejador de evento 'submit' del formulario de forma segura
    $(document).off('submit', '#formPost').on('submit', '#formPost', function (event) {
        event.preventDefault(); // Previene el envío normal del formulario

        var formData = new FormData(this); // Crea un objeto FormData a partir del formulario

        $.ajax({
            type: 'POST',
            url: $(this).attr('action'), // Usa la URL del formulario
            data: formData, // Usa el FormData
            processData: false, // Evita que jQuery procese los datos
            contentType: false, // Evita que jQuery establezca el content type
            success: function (result) {
                // Si hay un redireccionamiento, recargar la página o ir a la URL
                if (result.redirect) {
                    window.location.href = result.redirectUrl;
                } else {
                    // Actualiza el contenido del modal con el resultado devuelto
                    $('#modalBody').html(result);

                    // Reaplicar el tamaño del modal si es "Detalles"
                    if (title === "Detalles") {
                        document.getElementById('ModalSize').classList.add('modal-xl');
                    } else {
                        document.getElementById('ModalSize').classList.remove('modal-xl');
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                // Mostrar el contenido del error en el modal
                $('#modalBody').html(xhr.responseText);
            }
        });
    });
}