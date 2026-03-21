const timeoutMs = 5 * 60 * 1000;
let timer;
let modalMostrado = false;

function mostrarModalSesionExpirada() {
    if (modalMostrado) return;
    modalMostrado = true;

    const modal = document.getElementById("modalSesionExpirada");
    const mensaje = document.getElementById("mensajeSesionExpirada");

    if (mensaje) {
        mensaje.textContent = "La sesión expiró por inactividad.";
    }

    if (modal) {
        modal.classList.add("activo");
        document.body.style.pointerEvents = "none";
        modal.style.pointerEvents = "all";
    } else {
        window.location.href = "/Index";
    }
}

function redirigirIndex() {
    window.location.href = "/Index";
}

function resetTimer() {
    if (modalMostrado) return;

    clearTimeout(timer);

    timer = setTimeout(() => {
        fetch('/Index?handler=SetSessionExpired', {
            method: 'POST'
        }).finally(() => {
            mostrarModalSesionExpirada();
        });
    }, timeoutMs);
}

window.addEventListener("load", resetTimer);
document.addEventListener("mousemove", resetTimer);
document.addEventListener("keydown", resetTimer);
document.addEventListener("click", resetTimer);
document.addEventListener("scroll", resetTimer);