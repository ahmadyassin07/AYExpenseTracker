// service-worker.js
// 1. Import Firebase libraries (compat version) for background messaging
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging-compat.js');
importScripts('js/firebase-config.js');

// 2. Initialize Firebase Messaging in Service Worker
if (typeof firebaseConfig !== 'undefined') {
    firebase.initializeApp(firebaseConfig);
    const messaging = firebase.messaging();

    // Handle background notifications
    messaging.onBackgroundMessage((payload) => {
        console.log('[service-worker.js] Background message received:', payload);
        if (!payload.notification) return;

        const title = payload.notification.title || "AYExpense";
        const options = {
            body: payload.notification.body || "",
            icon: 'wallet.png',
            badge: 'wallet.png',
            data: payload.data || {}
        };
        self.registration.showNotification(title, options);
    });
}

// 3. Notification Click Handler
self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    const url = event.notification?.data?.url || './';
    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(windowClients => {
            for (let client of windowClients) {
                if (client.url.includes(url) && 'focus' in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) return clients.openWindow(url);
        })
    );
});

self.addEventListener('install', event => {
    console.log('Service Worker installing...');
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    console.log('Service Worker activated.');
    event.waitUntil(self.clients.claim());
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});

self.addEventListener('message', (event) => {
    if (event.data === 'skipWaiting') {
        self.skipWaiting();
    }
});