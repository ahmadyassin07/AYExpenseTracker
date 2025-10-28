





// push-notifications.js
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import {
    getMessaging,
    getToken,
    deleteToken,
    onMessage
} from "https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging.js";

let messaging;

/**
 * Initialize Firebase app and messaging
 * @param {Object} firebaseConfig - Your Firebase config
 */
export function initialize(firebaseConfig) {
    const app = initializeApp(firebaseConfig);
    messaging = getMessaging(app);

    // Listen for foreground messages
    onMessage(messaging, (payload) => {
        console.log('💬 Foreground message received:', payload);
        if (payload.notification) {
            alert(`🔔 ${payload.notification.title}\n${payload.notification.body}`);
        }
    });

    console.log("✅ Firebase initialized for messaging.");
}

/**
 * Request notification permission and get a fresh FCM token
 * @param {string} vapidKey - Your Firebase Web Push VAPID key
 */
export async function requestPermissionAndGetToken(vapidKey) {
    if (!messaging) {
        console.error("❌ Messaging not initialized. Call initialize(firebaseConfig) first.");
        return null;
    }

    try {
        console.log("📨 Checking notification permission...");

        if (Notification.permission === "denied") {
            alert("❌ Notifications are blocked. Re-enable them in site settings.");
            return null;
        }

        let permission = Notification.permission;
        if (permission !== "granted") {
            permission = await Notification.requestPermission();
            console.log("🔔 User responded with:", permission);
        }

        if (permission !== "granted") {
            alert("⚠️ Notifications not granted.");
            return null;
        }

        console.log("✅ Notification permission granted.");

        // Register service worker
        const registration = await navigator.serviceWorker.register('/AYExpenseTracker/firebase-messaging-sw.js');
        console.log("✅ Service worker registered:", registration);

        // Delete old token (if exists)
        try {
            const oldToken = await getToken(messaging, { vapidKey, serviceWorkerRegistration: registration });
            if (oldToken) {
                const deleted = await deleteToken(messaging, { serviceWorkerRegistration: registration });
                if (deleted) console.log("🔁 Old FCM token deleted.");
            }
        } catch (err) {
            console.warn("ℹ️ Could not delete old token (maybe none exists):", err);
        }

        // Get a new FCM token
        const token = await getToken(messaging, { vapidKey, serviceWorkerRegistration: registration });
        if (!token) {
            console.warn("⚠️ No FCM token received. Check HTTPS and VAPID key.");
            return null;
        }

        console.log("✅ New FCM token obtained:", token);
        return token;

    } catch (err) {
        console.error("❌ Error requesting permission or getting token:", err);
        if (Notification.permission === "denied") {
            alert("❌ Notifications are blocked. Re-enable in site settings.");
        }
        return null;
    }
}