function abrirEditarParametro(btn) {
    const p = JSON.parse(btn.getAttribute('data-parametro'));
    document.getElementById('modalTitulo').innerText = 'Editar Parámetro';
    document.getElementById('fParamIdOriginal').value = p.parametroId;
    document.getElementById('fParamId').value = p.parametroId;
    document.getElementById('fParamId').readOnly = true;
    document.getElementById('fParamValor').value = p.valor;
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalParametro')).show();
}

function abrirNuevoParametro() {
    document.getElementById('modalTitulo').innerText = 'Nuevo Parámetro';
    document.getElementById('fParamIdOriginal').value = '';
    document.getElementById('fParamId').value = '';
    document.getElementById('fParamId').readOnly = false;
    document.getElementById('fParamValor').value = '';
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalParametro')).show();
}