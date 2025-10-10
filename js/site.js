function downloadFileFromBlazor(filename, base64) {
    const link = document.createElement('a');
    link.href = 'data:text/csv;base64,' + base64;
    link.download = filename;
    link.click();
}
// Disable right-click on entire website
//document.addEventListener('contextmenu', function (e) {
//  e.preventDefault(); // Prevents right-click menu
//});









function renderPieChart(id, labels, data) {
    const ctx = document.getElementById(id);
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: ['#00c6ff', '#f9a826', '#ff5f6d', '#42e695', '#8a2be2'],
                borderWidth: 1
            }]
        },
        options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
    });
}

function renderLineChart(id, labels, data) {
    const ctx = document.getElementById(id);
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Monthly Spending',
                data: data,
                borderColor: '#00c6ff',
                tension: 0.3,
                fill: true,
                backgroundColor: 'rgba(0,198,255,0.15)'
            }]
        },
        options: { responsive: true, plugins: { legend: { display: false } } }
    });
}



window.exportInsightsPDF = async function () {
    try {
        const element = document.querySelector('.insights-container');
        if (!element) return;

        // Render the element to canvas
        const canvas = await html2canvas(element, { scale: 2, useCORS: true });
        const imgData = canvas.toDataURL('image/png');

        const pdf = new window.jspdf.jsPDF('p', 'mm', 'a4');
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = pdf.internal.pageSize.getHeight();

        const imgProps = pdf.getImageProperties(imgData);
        const imgHeight = (imgProps.height * pdfWidth) / imgProps.width;

        let heightLeft = imgHeight;
        let position = 0;

        // Add pages
        pdf.addImage(imgData, 'PNG', 0, position, pdfWidth, imgHeight);
        heightLeft -= pdfHeight;

        while (heightLeft > 0) {
            position = heightLeft - imgHeight;
            pdf.addPage();
            pdf.addImage(imgData, 'PNG', 0, position, pdfWidth, imgHeight);
            heightLeft -= pdfHeight;
        }

        pdf.save('insights.pdf');
    } catch (err) {
        console.error("PDF export failed:", err);
    }
};