
// Use importScripts instead of import
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging-compat.js');

// ✅ Initialize Firebase
const firebaseConfig = {
    apiKey: "AIzaSyBclK8VdNQN8GUFVyADmX_--bWJSFb7-Dk",
    authDomain: "ayexpensetracker.firebaseapp.com",
    projectId: "ayexpensetracker",
    storageBucket: "ayexpensetracker.firebasestorage.app",
    messagingSenderId: "730959947369",
    appId: "1:730959947369:web:cd8e38bcac983702d864f8"
};


const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

// ✅ Handle background notifications
onBackgroundMessage(messaging, (payload) => {
    console.log('[firebase-messaging-sw.js] Background message received:', payload);

    if (!payload.notification) {
        console.warn("⚠️ No notification payload found.");
        return;
    }

    const notificationTitle = payload.notification.title || "Notification";
    const notificationOptions = {
        body: payload.notification.body || "",
        icon: '/wallet.png',
        badge: '/wallet.png',
        data: payload.data || {}
    };

    // ✅ Show the system notification
    self.registration.showNotification(notificationTitle, notificationOptions);
});

// ✅ (Optional) Handle notification clicks (this part stays the same)
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