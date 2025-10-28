// Import modular Firebase SDK
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging.js";

let messaging;

/**
 * Initialize Firebase App & Messaging
 * @param {Object} firebaseConfig
 */
export function initializeFirebase(firebaseConfig) {
    const app = initializeApp(firebaseConfig);
    messaging = getMessaging(app);

    // Handle foreground messages
    onMessage(messaging, (payload) => {
        console.log("💬 Foreground message received:", payload);

        const title = payload.notification?.title || payload.data?.title || "Notification";
        const body = payload.notification?.body || payload.data?.body || "";

        if (title && body) {
            alert(`🔔 ${title}\n${body}`);
        }
    });

    console.log("✅ Firebase initialized for messaging");
}

/**
 * Request notification permission & get FCM token
 * @param {string} vapidKey - Your Web Push VAPID key
 */
export async function requestPermissionAndGetToken(vapidKey) {
    if (!messaging) throw new Error("Firebase not initialized. Call initializeFirebase() first.");

    try {
        console.log("📨 Requesting notification permission...");
        const permission = await Notification.requestPermission();
        console.log("🔔 Permission result:", permission);

        if (permission !== "granted") {
            alert("⚠️ Notification permission denied. Enable to receive updates.");
            return null;
        }

        // Register Service Worker
        const registration = await navigator.serviceWorker.register("/AYExpenseTracker/firebase-messaging-sw.js");
        console.log("✅ Service worker registered:", registration);

        // Get FCM token
        const token = await getToken(messaging, {
            vapidKey: vapidKey,
            serviceWorkerRegistration: registration
        });

        if (!token) {
            console.warn("⚠️ No FCM token received.");
            return null;
        }

        console.log("✅ FCM token obtained:", token);
        return token;

    } catch (err) {
        console.error("❌ Error getting FCM token:", err);
        return null;
    }
}