// Give the service worker access to Firebase Messaging.
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/9.6.1/firebase-messaging-compat.js');

// Initialize the Firebase app in the service worker with your project's config
const firebaseConfig = {
    apiKey: "AIzaSyBclK8VdNQN8GUFVyADmX_--bWJSFb7-Dk", // From Project Settings -> General
    authDomain: "ayexpensetracker.firebaseapp.com",
    projectId: "ayexpensetracker",
    storageBucket: "ayexpensetracker.firebasestorage.app",
    messagingSenderId: "730959947369",
    appId: "1:730959947369:web:cd8e38bcac983702d864f8"
};

firebase.initializeApp(firebaseConfig);

const messaging = firebase.messaging();

// This handler will be called when a message is received while the app is in the background
messaging.onBackgroundMessage(function (payload) {
    console.log('[firebase-messaging-sw.js] Received background message ', payload);

    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
        icon: '/wallet.png' // Optional: path to your app's icon
    };

    self.registration.showNotification(notificationTitle, notificationOptions);
});