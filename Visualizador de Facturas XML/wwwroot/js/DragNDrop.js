const dropArea = document.getElementById('drop-area');
const fileInput = document.getElementById('fileElem');

// Prevenir comportamiento predeterminado (como abrir el archivo)
['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    dropArea.addEventListener(eventName, preventDefaults, false);
    document.body.addEventListener(eventName, preventDefaults, false); // Para manejar el drag and drop fuera del área
});

function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

// Agregar y eliminar la clase "highlight" al arrastrar
['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    dropArea.addEventListener(eventName, highlight, false);
});

function highlight(e) {
    if (e.type === 'dragover') {
        dropArea.classList.add('highlight');
    } else {
        dropArea.classList.remove('highlight');
    }
}

// Manejar el archivo cuando se suelta
dropArea.addEventListener('drop', handleDrop, false);

function handleDrop(e) {
    const dt = e.dataTransfer;
    const files = dt.files;

    handleFiles(files);
}

// Manejar la selección de archivos
dropArea.addEventListener('click', () => {
    fileInput.click(); // Simula un clic en el input de archivo
});

// Manejar archivos seleccionados
fileInput.addEventListener('change', () => {
    handleFiles(fileInput.files);
});

function handleFiles(files) {
    // Aquí puedes agregar tu lógica para manejar los archivos
    console.log(files); // Solo para visualizar en la consola
    if (files.length > 0) {
        dropArea.querySelector('p').textContent = `Archivo seleccionado: ${files[0].name}`;
    }
}