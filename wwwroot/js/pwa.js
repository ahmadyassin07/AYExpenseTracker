let deferredPrompt;
let dotNetHelper;

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('SetInstallable', true);
    }
});

window.initPwa = (helper) => {
    dotNetHelper = helper;
    
    // If prompt already fired before helper was ready, notify now
    if (deferredPrompt && dotNetHelper) {
        dotNetHelper.invokeMethodAsync('SetInstallable', true);
    }

    // 2. Handle Update Available (Service Worker)
    if ('serviceWorker' in navigator) {
        // Handle the reload once the new service worker takes control
        let refreshing = false;
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (refreshing) return;
            refreshing = true;
            window.location.reload();
        });

        navigator.serviceWorker.ready.then(registration => {
            registration.addEventListener('updatefound', () => {
                const newWorker = registration.installing;
                newWorker.addEventListener('statechange', () => {
                    if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('SetUpdateAvailable', true);
                        }
                    }
                });
            });
        });
    }

    // 3. Detect iOS & Notifications
    const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
    const isStandalone = window.matchMedia('(display-mode: standalone)').matches;
    const notifPermission = ("Notification" in window) ? Notification.permission : "default";

    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('SetPlatformInfo', isIOS, isStandalone);
        dotNetHelper.invokeMethodAsync('SetNotificationPermission', notifPermission);
    }
};

window.triggerPwaInstall = async () => {
    if (!deferredPrompt) return;
    deferredPrompt.prompt();
    const { outcome } = await deferredPrompt.userChoice;
    deferredPrompt = null;
    return outcome === 'accepted';
};

window.triggerPwaUpdate = () => {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.getRegistration().then(reg => {
            if (reg && reg.waiting) {
                reg.waiting.postMessage('skipWaiting');
            } else {
                // fallback if no waiting worker, just reload
                window.location.reload();
            }
        });
    } else {
        window.location.reload();
    }
};

