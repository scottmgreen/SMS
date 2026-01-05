// SMS3 File Viewer JavaScript Functions

/**
 * Download file from base64 data
 * @param {string} fileName - Name of the file
 * @param {string} mimeType - MIME type of the file
 * @param {string} base64Data - Base64 encoded file data
 */
window.downloadFile = function(fileName, mimeType, base64Data) {
    try {
        // Convert base64 to blob
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);
        
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: mimeType });
        
        // Create download link
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        
        // Trigger download
        document.body.appendChild(link);
        link.click();
        
        // Cleanup
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
        
        console.log(`Download initiated for: ${fileName}`);
    } catch (error) {
        console.error('Error downloading file:', error);
        alert('Error downloading file: ' + error.message);
    }
};

/**
 * Open file in new tab from base64 data
 * @param {string} dataUrl - Data URL of the file (data:mime;base64,...)
 * @param {string} fileName - Name of the file (for tab title)
 */
window.openFileInNewTab = function(dataUrl, fileName) {
    try {
        const newWindow = window.open();
        if (newWindow) {
            newWindow.document.title = fileName || 'File Viewer';
            newWindow.location.href = dataUrl;
            console.log(`Opened file in new tab: ${fileName}`);
        } else {
            // Popup blocked
            alert('Popup blocked. Please allow popups for this site to open files in new tabs.');
        }
    } catch (error) {
        console.error('Error opening file in new tab:', error);
        alert('Error opening file in new tab: ' + error.message);
    }
};

/**
 * Check if browser supports file viewing
 * @param {string} mimeType - MIME type to check
 * @returns {boolean} - True if supported
 */
window.canViewFileType = function(mimeType) {
    // List of MIME types that browsers can generally display
    const supportedTypes = [
        'image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/bmp', 'image/webp', 'image/svg+xml',
        'application/pdf',
        'video/mp4', 'video/webm', 'video/ogg',
        'audio/mpeg', 'audio/wav', 'audio/ogg', 'audio/mp4',
        'text/plain', 'text/html', 'text/css', 'text/javascript', 'application/json'
    ];
    
    return supportedTypes.includes(mimeType.toLowerCase());
};

/**
 * Copy file data URL to clipboard
 * @param {string} dataUrl - Data URL to copy
 */
window.copyFileToClipboard = function(dataUrl) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(dataUrl).then(function() {
            console.log('File data URL copied to clipboard');
        }).catch(function(error) {
            console.error('Error copying to clipboard:', error);
        });
    } else {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = dataUrl;
        document.body.appendChild(textArea);
        textArea.select();
        try {
            document.execCommand('copy');
            console.log('File data URL copied to clipboard (fallback)');
        } catch (error) {
            console.error('Error copying to clipboard (fallback):', error);
        }
        document.body.removeChild(textArea);
    }
};

/**
 * Get file info from a file input element
 * @param {HTMLInputElement} fileInput - File input element
 * @returns {Array} - Array of file info objects
 */
window.getFileInfo = function(fileInput) {
    const files = [];
    if (fileInput.files) {
        for (let i = 0; i < fileInput.files.length; i++) {
            const file = fileInput.files[i];
            files.push({
                name: file.name,
                size: file.size,
                type: file.type,
                lastModified: file.lastModified
            });
        }
    }
    return files;
};

// Initialize file viewer functions
document.addEventListener('DOMContentLoaded', function() {
    console.log('SMS3 File Viewer JavaScript loaded');
});