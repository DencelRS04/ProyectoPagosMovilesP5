const timeoutMs = 5 * 60 * 1000;

let timer;

// 🔥 REINICIA EL CONTADOR
function resetTimer() {
    clearTimeout(timer);

    timer = setTimeout(() => {

        // 🔥 LLAMA AL BACKEND PARA GUARDAR MENSAJE
        fetch('/Auth/Login?handler=SetSessionExpired', {
            method: 'POST'
        })
            .finally(() => {
                alert("La sesión expiró por inactividad.");
                window.location.href = "/";
            });

    }, timeoutMs);
}

// 🔥 EVENTOS DE ACTIVIDAD
window.onload = resetTimer;
document.onmousemove = resetTimer;
document.onkeydown = resetTimer;
document.onclick = resetTimer;
document.onscroll = resetTimer;