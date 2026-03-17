let videoStream;

async function startCamera() {
    try {
        const video = document.getElementById('camera');
        if (!video) {
            console.warn("No video element with id='camera' found.");
            return;
        }

        if (videoStream) {
            videoStream.getTracks().forEach(track => track.stop());
            videoStream = null;
        }

        const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent);

        const constraints = {
            video: isMobile
                ? { facingMode: { ideal: "environment" }, width: { ideal: 1920 }, height: { ideal: 1080 } }
                : { facingMode: "user" },
            audio: false
        };

        try {
            videoStream = await navigator.mediaDevices.getUserMedia(constraints);
        } catch (err) {
            console.warn("Preferred camera not available, trying default.", err);
            videoStream = await navigator.mediaDevices.getUserMedia({ video: true });
        }

        video.srcObject = videoStream;
        await video.play();

        console.log(`Camera started (${isMobile ? "rear camera" : "front camera"})`);
    } catch (err) {
        console.error("Error starting camera:", err);
        throw err; // let Blazor know camera failed
    }
}

function stopCamera() {
    if (videoStream) {
        videoStream.getTracks().forEach(track => track.stop());
        videoStream = null;
    }
}

function capturePhoto() {
    return new Promise((resolve, reject) => {
        try {
            const video = document.getElementById('camera');
            const canvas = document.getElementById('canvas');

            if (!video || !canvas) {
                reject("Missing video or canvas element.");
                return;
            }

            const context = canvas.getContext('2d');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;

            // Enhance contrast slightly for OCR
            context.filter = 'contrast(1.1) brightness(1.05)';
            context.drawImage(video, 0, 0, canvas.width, canvas.height);

            const dataUrl = canvas.toDataURL('image/png');

            if (videoStream) {
                videoStream.getTracks().forEach(track => track.stop());
                videoStream = null;
            }

            resolve(dataUrl);
        } catch (error) {
            console.error("Error capturing photo:", error);
            reject(error);
        }
    });
}

/**
 * Read a file input element as base64 data URL
 * @param {string} inputId - id of the <input type="file"> element
 */
function readFileAsBase64(inputId) {
    return new Promise((resolve, reject) => {
        const input = document.getElementById(inputId);
        if (!input || !input.files || !input.files[0]) {
            reject("No file selected");
            return;
        }

        const file = input.files[0];
        const reader = new FileReader();
        reader.onload = e => resolve(e.target.result);
        reader.onerror = e => reject("FileReader error: " + e.target.error);
        reader.readAsDataURL(file);
    });
}
/**
 * Trigger a click on a DOM element by id
 * @param {string} elementId 
 */
function clickElement(elementId) {
    const el = document.getElementById(elementId);
    if (el) {
        el.click();
    }
}

/**
 * Initialize drag and drop for a specific element
 * @param {string} elementId 
 * @param {object} dotNetHelper 
 */
function initDropZone(elementId, dotNetHelper) {
    const zone = document.getElementById(elementId);
    if (!zone) return;

    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        zone.addEventListener(eventName, e => {
            e.preventDefault();
            e.stopPropagation();
        });
    });

    zone.addEventListener('drop', e => {
        const files = e.dataTransfer.files;
        if (files && files[0]) {
            const reader = new FileReader();
            reader.onload = async event => {
                await dotNetHelper.invokeMethodAsync('HandleDroppedFile', event.target.result);
            };
            reader.readAsDataURL(files[0]);
        }
    });
}
