let videoStream;

async function startCamera() {
    try {
        const video = document.getElementById('camera');
        if (!video) {
            console.error("❌ No video element with id='camera' found.");
            return;
        }

        videoStream = await navigator.mediaDevices.getUserMedia({ video: true });
        video.srcObject = videoStream;
        await video.play();

        console.log("✅ Camera started successfully");
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

            // stop camera after capture
            if (videoStream) {
                videoStream.getTracks().forEach(track => track.stop());
                console.log("📸 Camera stopped after capture");
            }

            resolve(dataUrl);
        } catch (error) {
            console.error("Error capturing photo:", error);
            reject(error);
        }
    });
}
