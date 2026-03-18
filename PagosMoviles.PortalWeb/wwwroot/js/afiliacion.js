document.addEventListener("DOMContentLoaded", function () {
    const identificacion = document.getElementById("identificacion");
    const telefono = document.getElementById("telefono");
    const numeroCuenta = document.getElementById("numeroCuenta");

    if (identificacion) {
        identificacion.addEventListener("input", function () {
            this.value = this.value.replace(/\D/g, "").slice(0, 9);
        });
    }

    if (telefono) {
        telefono.addEventListener("input", function () {
            this.value = this.value.replace(/\D/g, "").slice(0, 8);
        });
    }

    if (numeroCuenta) {
        numeroCuenta.addEventListener("input", function () {
            this.value = this.value.replace(/[^a-zA-Z0-9]/g, "").slice(0, 25);
        });
    }
});