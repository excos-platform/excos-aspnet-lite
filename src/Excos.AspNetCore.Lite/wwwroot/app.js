// Get the current path prefix from the URL
function getPathPrefix() {
    const path = window.location.pathname;
    // Assume the prefix is the first segment (e.g., /excos)
    const match = path.match(/^\/[^\/]+/);
    return match ? match[0] : '';
}

// Check API status
document.getElementById('check-status').addEventListener('click', async function() {
    const statusResult = document.getElementById('status-result');
    statusResult.className = '';
    statusResult.textContent = 'Checking...';
    statusResult.style.display = 'block';
    
    try {
        const pathPrefix = getPathPrefix();
        const response = await fetch(`${pathPrefix}/api/status`);
        const data = await response.json();
        
        if (response.ok) {
            statusResult.className = 'success';
            statusResult.innerHTML = `
                <strong>API Status: Online</strong><br>
                Status: ${data.status}<br>
                Version: ${data.version}
            `;
        } else {
            statusResult.className = 'error';
            statusResult.textContent = `Error: ${data.error || 'Unknown error'}`;
        }
    } catch (error) {
        statusResult.className = 'error';
        statusResult.textContent = `Failed to connect to API: ${error.message}`;
    }
});

// Display current route info
console.log('Excos Plugin loaded successfully');
console.log('Path prefix:', getPathPrefix());
