// Function to capture the actual RadzenQRCode from the DOM
window.captureRadzenQRCode = function () {
    try {
        console.log('🔍 Looking for RadzenQRCode...');

        // RadzenQRCode generates an SVG with class "rz-qrcode"
        const qrCodeSvg = document.querySelector('svg.rz-qrcode');

        if (qrCodeSvg) {
            console.log('✅ Found RadzenQRCode SVG:', qrCodeSvg);

            // Convert SVG to data URL
            const serializer = new XMLSerializer();
            const svgString = serializer.serializeToString(qrCodeSvg);
            const dataUrl = 'data:image/svg+xml;base64,' + btoa(unescape(encodeURIComponent(svgString)));

            console.log('📸 SVG converted to data URL, length:', dataUrl.length);
            return dataUrl;
        }

        console.warn('❌ No RadzenQRCode SVG found with selector: svg.rz-qrcode');
        return null;
    } catch (error) {
        console.error('💥 Error capturing QR code:', error);
        return null;
    }
};

// Enhanced print function that uses the captured RadzenQRCode
window.printReportConfirmation = async function (reportId, hazardId, trackingId, submissionDate, trackingUrl) {
    try {
        console.log('🖨️ Starting print confirmation...');

        // Wait a moment for QR code to fully render
        await new Promise(resolve => setTimeout(resolve, 500));

        // Capture the actual QR code from the page
        const qrCodeDataUrl = window.captureRadzenQRCode();
        console.log('QR Code captured:', qrCodeDataUrl ? 'SUCCESS' : 'FAILED');

        // Build the print content using string concatenation
        var printContent = '<!DOCTYPE html>' +
            '<html>' +
            '<head>' +
            '<meta charset="utf-8">' +
            '<title>Hazard Report Confirmation - ' + trackingId + '</title>' +
            '<style>' +
            'body { font-family: Arial, sans-serif; margin: 20px; color: #333; background: white; }' +
            '.header { text-align: center; border-bottom: 2px solid #212e61; padding-bottom: 20px; margin-bottom: 30px; }' +
            '.logo { color: #212e61; font-size: 24px; font-weight: bold; margin-bottom: 10px; }' +
            '.subtitle { color: #666; font-size: 14px; }' +
            '.content { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 30px; }' +
            '.details { flex: 1; margin-right: 20px; }' +
            '.qr-section { text-align: center; flex-shrink: 0; width: 150px; }' +
            '.detail-row { margin: 10px 0; padding: 5px 0; border-bottom: 1px dotted #ccc; }' +
            '.label { font-weight: bold; color: #212e61; display: inline-block; width: 120px; }' +
            '.qr-code { margin: 10px 0; }' +
            '.qr-code img { width: 120px; height: 120px; border: 1px solid #ddd; padding: 5px; background: white; }' +
            '.not-available { width: 120px; height: 120px; border: 2px dashed #ccc; display: flex; align-items: center; justify-content: center; font-size: 12px; color: #666; text-align: center; margin: 0 auto; background: #f9f9f9; }' +
            '.tracking-note { background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 5px; padding: 15px; margin: 20px 0; }' +
            '.footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; font-size: 12px; color: #666; }' +
            '@@media print { body { margin: 0 !important; padding: 15px !important; background: white !important; } .content { page-break-inside: avoid !important; } }' +
            '</style>' +
            '</head>' +
            '<body>' +
            '<div class="header">' +
            '<div class="logo">🛡️ PDXSMS</div>' +
            '<div class="subtitle">Port of Portland Safety Management System</div>' +
            '<h2 style="color: #0d8944; margin: 20px 0;">Hazard Report Confirmation</h2>' +
            '</div>' +
            '<div class="content">' +
            '<div class="details">' +
            '<div class="detail-row"><span class="label">Tracking ID:</span><strong>' + trackingId + '</strong></div>' +
            '<div class="detail-row"><span class="label">Report ID:</span>' + reportId + '</div>' +
            '<div class="detail-row"><span class="label">Hazard ID:</span>' + hazardId + '</div>' +
            '<div class="detail-row"><span class="label">Submitted:</span>' + submissionDate + '</div>' +
            '<div class="detail-row"><span class="label">Tracking URL:</span><br><small style="word-break: break-all;">' + trackingUrl + '</small></div>' +
            '</div>' +
            '<div class="qr-section">' +
            '<strong>Track Your Report</strong>' +
            '<div class="qr-code">' +
            (qrCodeDataUrl ? '<img src="' + qrCodeDataUrl + '" alt="QR Code for ' + trackingUrl + '" />' : '<div class="not-available">QR Code<br>Not Available</div>') +
            '</div>' +
            '<div style="font-size: 12px; color: #666; margin-top: 5px;">Scan to track your report</div>' +
            '</div>' +
            '</div>' +
            '<div class="tracking-note">' +
            '<strong>📋 Important Information:</strong>' +
            '<ul style="margin: 10px 0 0 20px;">' +
            '<li>Keep this confirmation for your records</li>' +
            '<li>Use the Tracking ID to check report status</li>' +
            '<li>Your report will be reviewed by SMS personnel</li>' +
            '<li>You will be notified of any updates if contact information was provided</li>' +
            '</ul>' +
            '</div>' +
            '<div class="footer">' +
            '<p><strong>Port of Portland - Safety Management System</strong></p>' +
            '<p>Confidential and Protected Information</p>' +
            '<p>Generated on ' + new Date().toLocaleString() + '</p>' +
            '</div>' +
            '</body>' +
            '</html>';

        // Create print window and print
        const printWindow = window.open('', '_blank', 'width=800,height=900');
        if (!printWindow) {
            alert('Please allow popups for this site to enable printing.');
            return false;
        }

        printWindow.document.write(printContent);
        printWindow.document.close();

        // Wait for content to load, then print
        printWindow.onload = function () {
            setTimeout(() => {
                printWindow.focus();
                printWindow.print();

                printWindow.onafterprint = function () {
                    setTimeout(() => {
                        printWindow.close();
                    }, 100);
                };
            }, 100);
        };

        return true;
    } catch (error) {
        console.error('Error in printReportConfirmation:', error);
        alert('Print function encountered an error. Please try again.');
        return false;
    }
};

// Fallback print function
window.printContent = function (htmlContent) {
    try {
        const printWindow = window.open('', '_blank', 'width=800,height=900');
        if (!printWindow) {
            alert('Please allow popups for this site to enable printing.');
            return;
        }

        printWindow.document.write(htmlContent);
        printWindow.document.close();

        printWindow.onload = function () {
            setTimeout(() => {
                printWindow.focus();
                printWindow.print();
                printWindow.onafterprint = function () {
                    setTimeout(() => {
                        printWindow.close();
                    }, 100);
                };
            }, 100);
        };
    } catch (error) {
        console.error('Error in printContent:', error);
        alert('Print function encountered an error. Please try again.');
    }
};