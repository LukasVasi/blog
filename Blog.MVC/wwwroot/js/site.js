// Automatically closes bootstrap alerts
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('[data-autodismiss="true"]').forEach(function (alert) {
        const delay = parseInt(alert.dataset.autodismissDelay) || 3000;

        setTimeout(() => {
            bootstrap.Alert.getOrCreateInstance(alert).close();
        }, delay);
    });
});
