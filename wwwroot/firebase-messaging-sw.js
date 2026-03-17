// firebase-messaging-sw.js

// 1. Import Firebase libraries (compat versions)
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging-compat.js');

// 2. Import External Config (This hides the key from some simple automated repository scanners)
// Note: In a public PWA, the key remains technically public. 
// CRITICAL: You MUST restrict this API Key in the Google Cloud Console to your domain.
importScripts('js/firebase-config.js');

// Check if config loaded
if (typeof firebaseConfig !== 'undefined') {
    firebase.initializeApp(firebaseConfig);
    const messaging = firebase.messaging();

    // Handle background notifications
    messaging.onBackgroundMessage((payload) => {
        console.log('[firebase-messaging-sw.js] Background message received:', payload);

        if (!payload.notification) return;

        const notificationTitle = payload.notification.title || "Message";
        const notificationOptions = {
            body: payload.notification.body || "",
            icon: '/wallet.png',
            badge: '/wallet.png',
            data: payload.data || {}
        };

        self.registration.showNotification(notificationTitle, notificationOptions);
    });
} else {
    console.error("Firebase config not found. Background notifications will not work.");
}

// Handle notification clicks
self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    const targetUrl = event.notification?.data?.url || '/';
    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(windowClients => {
            for (let client of windowClients) {
                if (client.url.includes(targetUrl) && 'focus' in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow(targetUrl);
            }
        })
    );
});
