//import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
//import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging.js";

//let messaging;

//export function initialize(firebaseConfig) {
//    const app = initializeApp(firebaseConfig);
//    messaging = getMessaging(app);

//    // Handle messages when app is in the foreground
//    onMessage(messaging, (payload) => {
//        console.log('Foreground message received: ', payload);
//        alert(`Notification: ${payload.notification.title}\n${payload.notification.body}`);
//    });
//}

//export async function requestPermissionAndGetToken(vapidKey) {
//    try {
//        const permission = await Notification.requestPermission();
//        if (permission !== 'granted') {
//            console.log('Notification permission denied');
//            return null;
//        }

//        console.log('Notification permission granted.');

//        // Must pass service worker registration for mobile
//        const registration = await navigator.serviceWorker.ready;

//        const token = await getToken(messaging, {
//            vapidKey: vapidKey,
//            serviceWorkerRegistration: registration
//        });

//        console.log("FCM Token:", token);
//        return token;
//    } catch (err) {
//        console.error('Error getting FCM token', err);
//        return null;
//    }
//}


import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging.js";

let messaging;

/**
 * Initialize Firebase
 * @param {Object} firebaseConfig - Your Firebase config object
 */
export function initialize(firebaseConfig) {
    const app = initializeApp(firebaseConfig);
    messaging = getMessaging(app);

    // Handle messages when app is in the foreground
    onMessage(messaging, (payload) => {
        console.log('Foreground message received:', payload);

        if (payload.notification) {
            alert(`Notification:\n${payload.notification.title}\n${payload.notification.body}`);
        }
    });

    console.log("✅ Firebase initialized for messaging.");
}

/**
 * Request permission and get FCM token
 * Must be called from a user gesture (button click)
 * @param {string} vapidKey - Your Firebase VAPID key
 */
export async function requestPermissionAndGetToken(vapidKey) {
    try {
        // Request Notification permission
        const permission = await Notification.requestPermission();
        if (permission !== 'granted') {
            console.warn("⚠️ Notification permission denied by user.");
            return null;
        }
        console.log("✅ Notification permission granted.");

        // Explicitly register the service worker
        const registration = await navigator.serviceWorker.register('./firebase-messaging-sw.js');
        console.log("✅ Service worker registered:", registration);

        // Get FCM token
        const token = await getToken(messaging, {
            vapidKey: vapidKey,
            serviceWorkerRegistration: registration
        });

        if (token) {
            console.log("✅ FCM Token obtained:", token);
        } else {
            console.warn("⚠️ No FCM token received.");
        }

        return token;
    } catch (err) {
        console.error("❌ Error requesting permission or getting token:", err);
        return null;
    }
}