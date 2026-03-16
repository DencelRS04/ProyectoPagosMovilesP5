document.addEventListener("DOMContentLoaded", function () {
    const identificacion = document.getElementById("identificacion");

    if (identificacion) {
        identificacion.addEventListener("input", function () {
            this.value = this.value.replace(/\s+/g, "");
        });
    }
});