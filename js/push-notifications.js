import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging.js";

let messaging;

export function initialize(firebaseConfig) {
    const app = initializeApp(firebaseConfig);
    messaging = getMessaging(app);

    // Handle messages when app is in the foreground
    onMessage(messaging, (payload) => {
        console.log('Foreground message received: ', payload);
        alert(`Notification: ${payload.notification.title}\n${payload.notification.body}`);
    });
}

export async function requestPermissionAndGetToken(vapidKey) {
    try {
        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            console.log('Notification permission denied');
            return null;
        }

        console.log('Notification permission granted.');

        // Must pass service worker registration for mobile
        const registration = await navigator.serviceWorker.ready;

        const token = await getToken(messaging, {
            vapidKey: vapidKey,
            serviceWorkerRegistration: registration
        });

        console.log("FCM Token:", token);
        return token;
    } catch (err) {
        console.error('Error getting FCM token', err);
        return null;
    }
}
