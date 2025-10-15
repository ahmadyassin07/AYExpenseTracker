import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging.js";

let messaging;

// Initializes Firebase and the messaging service
export function initialize(firebaseConfig) {
    const app = initializeApp(firebaseConfig);
    messaging = getMessaging(app);

    // Optional: handle messages while the app is in the foreground
    onMessage(messaging, (payload) => {
        console.log('Message received while app is in foreground. ', payload);
        // You could show a custom in-app toast here instead of a system notification
        alert(`Notification: ${payload.notification.title}\n${payload.notification.body}`);
    });
}

// Requests permission and returns the FCM token if granted
export async function requestPermissionAndGetToken(vapidKey) {
    try {
        const permission = await Notification.requestPermission();
        if (permission === 'granted') {
            console.log('Notification permission granted.');
            // Get the token
            const fcmToken = await getToken(messaging, { vapidKey: vapidKey });
            if (fcmToken) {
                console.log("FCM Token:", fcmToken);
                return fcmToken;
            } else {
                console.log('No registration token available. Request permission to generate one.');
                return null;
            }
        } else {
            console.log('Unable to get permission to notify.');
            return null;
        }
    } catch (err) {
        console.error('An error occurred while retrieving token. ', err);
        return null;
    }
}