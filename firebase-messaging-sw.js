
// firebase-messaging-sw.js

// Import Firebase libraries for service workers (compat versions)
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging-compat.js');

// ✅ Initialize Firebase in the service worker
const firebaseConfig = {
    apiKey: "AIzaSyBclK8VdNQN8GUFVyADmX_--bWJSFb7-Dk", // Project Settings → General
    authDomain: "ayexpensetracker.firebaseapp.com",
    projectId: "ayexpensetracker",
    storageBucket: "ayexpensetracker.firebasestorage.app",
    messagingSenderId: "730959947369",
    appId: "1:730959947369:web:cd8e38bcac983702d864f8"
};

firebase.initializeApp(firebaseConfig);

// ✅ Get Firebase Messaging instance
const messaging = firebase.messaging();

// ✅ Handle background notifications
messaging.onBackgroundMessage((payload) => {
    console.log('[firebase-messaging-sw.js] Background message received:', payload);

    if (!payload.notification) {
        console.warn("⚠️ No notification payload found.");
        return;
    }

    const notificationTitle = payload.notification.title || "Notification";
    const notificationOptions = {
        body: payload.notification.body || "",
        icon: '/wallet.png', // optional icon
        badge: '/wallet.png', // optional badge
        data: payload.data || {}
    };

    // ✅ Show the system notification
    self.registration.showNotification(notificationTitle, notificationOptions);
});

// ✅ (Optional) Handle notification clicks
self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    const targetUrl = event.notification?.data?.url || '/AYExpenseTracker/';
    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(windowClients => {
            // If a window/tab matching the target URL is already open, focus it
            for (let client of windowClients) {
                if (client.url.includes(targetUrl) && 'focus' in client) {
                    return client.focus();
                }
            }
            // Otherwise, open a new tab/window
            if (clients.openWindow) {
                return clients.openWindow(targetUrl);
            }
        })
    );
});
