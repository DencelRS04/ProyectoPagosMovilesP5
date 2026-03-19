let tiempoInactividad;
const LIMITE_INACTIVIDAD = 5 * 60 * 1000; // 5 minutos

function reiniciarTemporizador() {

    clearTimeout(tiempoInactividad);

    tiempoInactividad = setTimeout(function () {

        fetch('/SessionExpired', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(() => {
                window.location.href = "/";
            })
            .catch(() => {
                window.location.href = "/";
            });

    }, LIMITE_INACTIVIDAD);
}

function iniciarControlSesion() {

    document.addEventListener("keydown", reiniciarTemporizador);
    document.addEventListener("click", reiniciarTemporizador);
    document.addEventListener("scroll", reiniciarTemporizador);

    reiniciarTemporizador();
}

window.addEventListener("load", iniciarControlSesion);