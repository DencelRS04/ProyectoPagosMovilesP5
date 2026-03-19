const timeoutMs = 5 * 60 * 1000;
let lastActivity = Date.now();
let expired = false;

function markActivity() {
    if (!expired) {
        lastActivity = Date.now();
    }
}

function expireSession() {
    if (expired) return;
    expired = true;

    fetch('/?handler=SetSessionExpired', {
        method: 'POST'
    })
        .finally(() => {
            alert("La sesión expiró por inactividad.");
            window.location.href = "/";
        });
}

// Registrar actividad del usuario
window.addEventListener("load", markActivity);
window.addEventListener("keydown", markActivity);
window.addEventListener("click", markActivity);
window.addEventListener("scroll", markActivity);

// Verificar inactividad cada segundoconst timeoutMs = 5 * 60 * 1000;
let lastActivity = Date.now();
let expired = false;

function markActivity() {
    if (!expired) {
        lastActivity = Date.now();
    }
}

function expireSession() {
    if (expired) return;
    expired = true;

    fetch('/?handler=SetSessionExpired', {
        method: 'POST'
    })
        .finally(() => {
            alert("La sesión expiró por inactividad.");
            window.location.href = "/";
        });
}

// Registrar actividad del usuario
window.addEventListener("load", markActivity);
window.addEventListener("keydown", markActivity);
window.addEventListener("click", markActivity);
window.addEventListener("scroll", markActivity);

// Verificar inactividad cada segundo
setInterval(function () {
    const inactiveTime = Date.now() - lastActivity;

    if (inactiveTime >= timeoutMs) {
        expireSession();
    }
}, 1000);
setInterval(function () {
    const inactiveTime = Date.now() - lastActivity;

    if (inactiveTime >= timeoutMs) {
        expireSession();
    }
}, 1000);