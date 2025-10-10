function downloadFileFromBlazor(filename, base64) {
    const link = document.createElement('a');
    link.href = 'data:text/csv;base64,' + base64;
    link.download = filename;
    link.click();
}
// Disable right-click on entire website
document.addEventListener('contextmenu', function (e) {
  e.preventDefault(); // Prevents right-click menu
});