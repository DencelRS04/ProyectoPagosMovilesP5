function abrirEditar(btn) {
    const usuario = JSON.parse(btn.getAttribute('data-usuario'));
    document.getElementById('modalTitulo').innerText = 'Editar Usuario';
    document.getElementById('fUserId').value = usuario.usuarioId;
    document.getElementById('Formulario_NombreCompleto').value = usuario.nombreCompleto;
    document.getElementById('Formulario_Email').value = usuario.email;
    document.getElementById('Formulario_Identificacion').value = usuario.identificacion;
    document.getElementById('Formulario_RolId').value = usuario.rolId;
    document.getElementById('Formulario_Telefono').value = '';
    document.getElementById('Formulario_Password').value = '';
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalUsuario')).show();
}

function abrirNuevo() {
    document.getElementById('modalTitulo').innerText = 'Nuevo Usuario';
    document.getElementById('fUserId').value = '';
    document.getElementById('Formulario_NombreCompleto').value = '';
    document.getElementById('Formulario_Email').value = '';
    document.getElementById('Formulario_Identificacion').value = '';
    document.getElementById('Formulario_Telefono').value = '';
    document.getElementById('Formulario_Password').value = '';
    document.getElementById('Formulario_RolId').value = '2';
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalUsuario')).show();
}