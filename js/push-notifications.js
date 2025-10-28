import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import {
    getMessaging,
    getToken,
    onMessage,
    deleteToken
} from "https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging.js";

let messaging;

/**
 * Initialize Firebase for messaging
 * @param {Object} firebaseConfig - Your Firebase config object
 */
export function initialize(firebaseConfig) {
    const app = initializeApp(firebaseConfig);
    messaging = getMessaging(app);

    // Handle foreground messages
    onMessage(messaging, (payload) => {
        console.log('💬 Foreground message received:', payload);

        if (payload.notification) {
            alert(`🔔 ${payload.notification.title}\n${payload.notification.body}`);
        }
    });

    console.log("✅ Firebase initialized for messaging.");
}

/**
 * Request permission, handle blocked cases, and get a fresh FCM token
 * @param {string} vapidKey - Your Firebase web push VAPID key
 */
//export async function requestPermissionAndGetToken(vapidKey) {
//    try {
//        console.log("📨 Checking notification permission...");

//        const permission = await Notification.requestPermission();
//        console.log("🔔 Permission result:", permission);

//        if (permission === 'denied') {
//            alert("❌ Notifications are blocked. To re-enable them:\n1. Click the 🔒 icon near the address bar.\n2. Go to 'Site settings'.\n3. Set Notifications → Allow.");
//            return null;
//        }

//        if (permission !== 'granted') {
//            alert("⚠️ Notifications not granted. Please allow to receive updates.");
//            return null;
//        }

//        console.log("✅ Notification permission granted.");

//        // Register service worker every time (ensures correct scope)
//        const registration = await navigator.serviceWorker.register('/AYExpenseTracker/firebase-messaging-sw.js');
//        console.log("✅ Service worker registered:", registration);

//        // Delete old token to force refresh
//        try {
//            await deleteToken(messaging);
//            console.log("🔁 Old FCM token deleted (forcing refresh).");
//        } catch {
//            console.log("ℹ️ No existing token found to delete.");
//        }

//        // Get a new FCM token
//        const token = await getToken(messaging, {
//            vapidKey: vapidKey,
//            serviceWorkerRegistration: registration
//        });

//        if (!token) {
//            console.warn("⚠️ No FCM token received.");
//            return null;
//        }

//        console.log("✅ New FCM token obtained:", token);
//        return token;

//    } catch (err) {
//        console.error("❌ Error requesting permission or getting token:", err);
//        return null;
//    }
//}
export async function requestPermissionAndGetToken(vapidKey) {
    try {
        console.log("📨 Checking notification permission...");

        // Check existing permission first before re-requesting
        if (Notification.permission === "denied") {
            alert("❌ Notifications are blocked. To re-enable them:\n1. Click the 🔒 icon near the address bar.\n2. Go to 'Site settings'.\n3. Set Notifications → Allow.");
            return null;
        }

        let permission = Notification.permission;

        // Only ask again if it's not already granted
        if (permission !== "granted") {
            permission = await Notification.requestPermission();
            console.log("🔔 User responded with:", permission);
        }

        if (permission === "denied") {
            alert("❌ Notifications are blocked. To re-enable them:\n1. Click the 🔒 icon near the address bar.\n2. Go to 'Site settings'.\n3. Set Notifications → Allow.");
            return null;
        }

        if (permission !== "granted") {
            alert("⚠️ Notifications not granted. Please allow them to receive updates.");
            return null;
        }

        console.log("✅ Notification permission granted.");

        // Register service worker properly
        const registration = await navigator.serviceWorker.register('/AYExpenseTracker/firebase-messaging-sw.js');
        console.log("✅ Service worker registered:", registration);

        // Try to delete old token safely
        try {
            const deleted = await deleteToken(messaging);
            if (deleted) console.log("🔁 Old FCM token deleted (refreshing).");
        } catch (err) {
            console.warn("ℹ️ No old FCM token to delete or error deleting:", err);
        }

        // Get a new FCM token
        const token = await getToken(messaging, {
            vapidKey,
            serviceWorkerRegistration: registration
        });

        if (!token) {
            console.warn("⚠️ No FCM token received (check HTTPS or VAPID key).");
            return null;
        }

        console.log("✅ New FCM token obtained:", token);
        return token;

    } catch (err) {
        console.error("❌ Error requesting permission or getting token:", err);

        // If error looks like blocked permission
        if (err.message?.includes("messaging") || Notification.permission === "denied") {
            alert("❌ Notifications are blocked or unavailable. Please re-enable in site settings.");
        }

        return null;
    }
}