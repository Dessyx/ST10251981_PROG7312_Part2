const ctx = document.getElementById('reportChart').getContext('2d');
const reportChart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
        datasets: [
            {
                label: 'Reports Submitted',
                data: [50, 70, 90, 120, 100, 130],
                borderColor: '#2f7fbf',
                backgroundColor: 'rgba(47, 127, 191, 0.2)',
                tension: 0.4,
                fill: true,
                pointRadius: 5,
                pointHoverRadius: 7
            },
            {
                label: 'Changes Made',
                data: [30, 50, 60, 90, 80, 100],
                borderColor: '#28a745',
                backgroundColor: 'rgba(40, 167, 69, 0.2)',
                tension: 0.4,
                fill: true,
                pointRadius: 5,
                pointHoverRadius: 7
            }
        ]
    },
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'top' },
            tooltip: { mode: 'index', intersect: false }
        },
        interaction: { mode: 'nearest', axis: 'x', intersect: false },
        scales: {
            y: {
                beginAtZero: true,
                title: {
                    display: true,
                    text: 'Number of Reports/Changes'
                }
            },
            x: {
                title: {
                    display: true,
                    text: 'Month'
                }
            }
        }
    }
});

