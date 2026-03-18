let videoStream;

async function startCamera() {
    try {
        const video = document.getElementById('camera');
        if (!video) {
            console.error("❌ No video element with id='camera' found.");
            return;
        }

        // Stop any existing stream first
        if (videoStream) {
            videoStream.getTracks().forEach(track => track.stop());
            videoStream = null;
            console.log("🛑 Previous camera stream stopped");
        }

        // Detect if device is mobile or desktop
        const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent);

        const constraints = {
            video: isMobile
                ? { facingMode: { ideal: "environment" } }
                : { facingMode: "user" },
            audio: false
        };

        try {
            videoStream = await navigator.mediaDevices.getUserMedia(constraints);
        } catch (err) {
            console.warn("⚠️ Preferred camera not available, using default.", err);
            videoStream = await navigator.mediaDevices.getUserMedia({ video: true });
        }

        video.srcObject = videoStream;
        await video.play();

        console.log(`✅ Camera started (${isMobile ? "mobile back camera" : "desktop camera"})`);
    } catch (err) {
        console.error("🚫 Error starting camera:", err);
        alert("Could not access camera. Please allow permissions and use HTTPS.");
    }
}

function capturePhoto() {
    return new Promise((resolve, reject) => {
        try {
            const video = document.getElementById('camera');
            const canvas = document.getElementById('canvas');

            if (!video || !canvas) {
                reject("❌ Missing video or canvas element.");
                return;
            }

            const context = canvas.getContext('2d');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            context.drawImage(video, 0, 0, canvas.width, canvas.height);

            const dataUrl = canvas.toDataURL('image/png');

            // Stop the camera after capturing
            if (videoStream) {
                videoStream.getTracks().forEach(track => track.stop());
                videoStream = null; // Clear the reference
                console.log("📸 Camera stopped after capture");
            }

            resolve(dataUrl);
        } catch (error) {
            console.error("Error capturing photo:", error);
            reject(error);
        }
    });
}
