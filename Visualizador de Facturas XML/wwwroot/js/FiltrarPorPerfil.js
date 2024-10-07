
    document.getElementById('filterInput').addEventListener('keyup', function () {
        var input = this.value.toLowerCase();
    var rows = document.querySelectorAll('#tableBody tr');

    rows.forEach(function (row) {
            var perfil = row.querySelector('.perfil-nombre').textContent.toLowerCase();
    if (perfil.includes(input)) {
        row.style.display = '';
            } else {
        row.style.display = 'none';
            }
        });
    });
