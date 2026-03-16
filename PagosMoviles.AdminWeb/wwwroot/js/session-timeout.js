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
    alert("La sesión ha expirado por inactividad.");
    window.location.href = "/";
}

window.addEventListener("load", function () {
    lastActivity = Date.now();
});

window.addEventListener("mousemove", markActivity);
window.addEventListener("keydown", markActivity);
window.addEventListener("click", markActivity);
window.addEventListener("scroll", markActivity);

setInterval(function () {
    const inactiveTime = Date.now() - lastActivity;
    if (inactiveTime >= timeoutMs) {
        expireSession();
    }
}, 1000);