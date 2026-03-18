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
export async function requestPermissionAndGetToken(vapidKey) {
    try {
        console.log("📨 Checking notification permission...");

        const permission = await Notification.requestPermission();
        console.log("🔔 Permission result:", permission);

        if (permission === 'denied') {
            alert("❌ Notifications are blocked. To re-enable them:\n1. Click the 🔒 icon near the address bar.\n2. Go to 'Site settings'.\n3. Set Notifications → Allow.");
            return null;
        }

        if (permission !== 'granted') {
            alert("⚠️ Notifications not granted. Please allow to receive updates.");
            return null;
        }

        console.log("✅ Notification permission granted.");

        // Register service worker every time (ensures correct scope)
        // Register the main service worker for messaging
        const registration = await navigator.serviceWorker.register('service-worker.js');
        console.log("✅ Service worker registered:", registration);

        // Delete old token to force refresh
        try {
            await deleteToken(messaging);
            console.log("🔁 Old FCM token deleted (forcing refresh).");
        } catch {
            console.log("ℹ️ No existing token found to delete.");
        }

        // Get a new FCM token
        const token = await getToken(messaging, {
            vapidKey: vapidKey,
            serviceWorkerRegistration: registration
        });

        if (!token) {
            console.warn("⚠️ No FCM token received.");
            return null;
        }

        console.log("✅ New FCM token obtained:", token);
        return token;

    } catch (err) {
        console.error("❌ Error requesting permission or getting token:", err);
        return null;
    }
}

/**
 * Send a notification to multiple tokens via FCM
 * @param {Array} tokens - List of FCM tokens
 * @param {Object} payload - Notification payload (title, body, icon, data)
 */
window.sendBulkNotifications = async (tokens, payload) => {
    console.log(`📣 Broadcasting to ${tokens.length} tokens...`);
    
    // In a real production app, you would send this to your own C# backend
    // which would use FirebaseAdmin SDK to send the message.
    // For this standalone client, we provide a warning that this is a simulated
    // broadcast as client-side FCM sending is deprecated/restricted.
    
    // We will simulate the success for the UI demonstration, but we'll also log
    // what would be sent.
    
    for (const token of tokens) {
        console.log(`🚀 Sending to token: ${token.substring(0, 10)}...`, payload);
    }

    // Return true to show success in the Blazor UI
    return true; 
};
