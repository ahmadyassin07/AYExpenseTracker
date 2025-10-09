function downloadFileFromBlazor(filename, base64) {
    const link = document.createElement('a');
    link.href = 'data:text/csv;base64,' + base64;
    link.download = filename;
    link.click();
}
 //Disable right-click on entire website
document.addEventListener('contextmenu', function (e) {
    e.preventDefault(); // Prevents right-click menu
});



// On page load, check redirect result
window.addEventListener('load', async () => {
    try {
        const result = await auth.getRedirectResult();
        if (result.user) {
            const idToken = await result.user.getIdToken();
            // Pass idToken back to Blazor via interop
            DotNet.invokeMethodAsync('AYExpenseTracker', 'ReceiveGoogleIdToken', idToken);
        }
    } catch (err) {
        console.error("Redirect Result Error:", err);
    }
});