/**
 * ?? SIMPLIFIED JSON Persistence JavaScript Client - NO AUTO-SAVE
 * ? MANUAL SAVE ONLY - Eliminates auto-save conflicts and 400 errors
 * ? User-controlled saves via Save Draft and Next Step buttons
 * ? Reliable, predictable, and user-friendly
 */

console.log('?? Loading SIMPLIFIED JSON Persistence Client - NO AUTO-SAVE');

class JsonPersistenceClient {
    constructor(assessmentId, stepNumber) {
        this.assessmentId = assessmentId;
        this.stepNumber = stepNumber;
        
        console.log(`? JsonPersistenceClient initialized for Assessment ${assessmentId}, Step ${stepNumber} - MANUAL SAVE ONLY`);
    }

    /**
     * Manual save method - called explicitly by save buttons
     */
    async manualSave() {
        try {
            console.log('?? Manual save triggered...');
            
            const formData = this.collectFormData();
            
            // Create form data for POST request
            const postData = new FormData();
            postData.append('Id', this.assessmentId);
            postData.append('StepNumber', this.stepNumber);
            postData.append('__RequestVerificationToken', this.getAntiForgeryToken());
            
            // Add form data
            Object.keys(formData).forEach(key => {
                postData.append(key, formData[key]);
            });
            
            // Use current page URL for save
            const currentPath = window.location.pathname;
            const saveUrl = `${currentPath}?handler=SaveStepData`;
            
            console.log(`?? Save URL: ${saveUrl}`);
            console.log(`?? Sending ${Object.keys(formData).length} form fields`);
            
            const response = await fetch(saveUrl, {
                method: 'POST',
                body: postData
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const result = await response.json();
            
            if (result.success) {
                console.log('? Manual save successful');
                this.showSaveIndicator('success', 'Saved successfully!');
                return { success: true, message: 'Saved successfully' };
            } else {
                console.error('? Save failed:', result.message);
                this.showSaveIndicator('error', 'Save failed');
                return { success: false, message: result.message };
            }
        } catch (error) {
            console.error('? Error in manual save:', error);
            this.showSaveIndicator('error', 'Save error');
            return { success: false, message: error.message };
        }
    }

    /**
     * Collect all form data for the current step
     */
    collectFormData() {
        const formData = {};
        
        console.log('?? Starting form data collection...');
        
        // Get all form inputs with names, excluding modal/stakeholder forms
        const inputs = document.querySelectorAll('input[name], textarea[name], select[name]');
        console.log(`?? Found ${inputs.length} total named form elements`);
        
        let collectedCount = 0;
        let skippedCount = 0;
        
        inputs.forEach((input, index) => {
            // Skip modal and stakeholder form inputs
            const isInModal = input.closest('.modal');
            const isStakeholderModal = input.closest('#createStakeholderModal');
            const isModalStakeholderField = input.name && (
                input.name === 'stakeholderName' ||
                input.name === 'stakeholderEmail' || 
                input.name === 'stakeholderOrganization' ||
                input.name === 'stakeholderGroup' ||
                input.name === 'stakeholderType' ||
                input.name === 'stakeholderCategory' ||
                input.name === 'stakeholderPhone'
            );
            
            if (isInModal || isStakeholderModal || isModalStakeholderField) {
                skippedCount++;
                console.log(`?? SKIPPED [${index}]: ${input.name} (${input.type}) - modal/stakeholder form`);
                return; // Skip excluded inputs
            }
            
            // Include this input
            let fieldValue;
            if (input.type === 'checkbox') {
                fieldValue = input.checked;
            } else if (input.type === 'radio') {
                if (input.checked) {
                    fieldValue = input.value;
                } else {
                    return; // Skip unchecked radio buttons
                }
            } else {
                fieldValue = input.value;
            }
            
            formData[input.name] = fieldValue;
            collectedCount++;
            
            // Log each collected field with truncated value for debugging
            const displayValue = typeof fieldValue === 'string' && fieldValue.length > 50 
                ? fieldValue.substring(0, 50) + '...' 
                : fieldValue;
            console.log(`?? COLLECTED [${index}]: ${input.name} (${input.type}) = "${displayValue}"`);
        });

        console.log(`?? Form data collection summary: ${collectedCount} collected, ${skippedCount} skipped`);
        
        // Log the final collected form data keys
        const collectedKeys = Object.keys(formData);
        console.log(`?? Final form data keys: ${collectedKeys.join(', ')}`);
        
        // Special logging for Step 1 fields to ensure they're being captured
        if (this.stepNumber === 1) {
            const step1Fields = ['SystemDescription', 'SystemBoundaries', 'SystemPurpose', 
                               'FiveMPersonnel', 'FiveMEquipment', 'FiveMProcedures', 
                               'FiveMResources', 'FiveMPhysicalEnvironment', 'FiveMOperationalEnvironment'];
            
            console.log('?? Step 1 field check:');
            step1Fields.forEach(fieldName => {
                if (formData[fieldName] !== undefined) {
                    const value = formData[fieldName];
                    const length = typeof value === 'string' ? value.length : 'N/A';
                    console.log(`  ? ${fieldName}: ${length} chars`);
                } else {
                    console.log(`  ? ${fieldName}: NOT FOUND`);
                }
            });
        }

        return formData;
    }

    /**
     * Get anti-forgery token for secure requests
     */
    getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    /**
     * Show visual indicator of save status
     */
    showSaveIndicator(status, message) {
        // Remove existing indicators
        const existingIndicator = document.querySelector('.save-indicator');
        if (existingIndicator) {
            existingIndicator.remove();
        }

        // Create new indicator
        const indicator = document.createElement('div');
        indicator.className = `save-indicator alert alert-${status === 'success' ? 'success' : 'danger'} position-fixed`;
        indicator.style.cssText = 'top: 20px; right: 20px; z-index: 9999; font-size: 0.9rem; padding: 0.75rem 1rem; opacity: 0.95; min-width: 200px;';
        
        const icon = status === 'success' ? '?' : '?';
        indicator.innerHTML = `${icon} ${message}`;
        document.body.appendChild(indicator);

        // Auto-remove after 3 seconds
        setTimeout(() => {
            if (indicator.parentNode) {
                indicator.parentNode.removeChild(indicator);
            }
        }, 3000);
    }

    /**
     * Initialize - NO AUTO-SAVE, just prepare for manual saves
     */
    initialize() {
        console.log('?? Initializing MANUAL SAVE ONLY - No auto-save listeners added');
        
        // Add manual save functionality to Save Draft buttons
        this.addSaveButtonListeners();
        
        console.log('? Manual save system ready');
    }

    /**
     * Add click listeners to Save Draft buttons
     */
    addSaveButtonListeners() {
        // Look for Save Draft buttons
        const saveDraftButtons = document.querySelectorAll('button[type="submit"][formmethod="post"][formaction*="SaveDraft"], .btn-save-draft, [data-action="save-draft"]');
        
        saveDraftButtons.forEach(button => {
            button.addEventListener('click', async (e) => {
                e.preventDefault();
                
                button.disabled = true;
                const originalText = button.innerHTML;
                button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Saving...';
                
                const result = await this.manualSave();
                
                button.disabled = false;
                button.innerHTML = originalText;
                
                if (result.success) {
                    console.log('? Save Draft completed successfully');
                } else {
                    console.error('? Save Draft failed:', result.message);
                }
            });
        });

        console.log(`?? Added manual save listeners to ${saveDraftButtons.length} Save Draft buttons`);
    }
}

/**
 * ?? GLOBAL FUNCTIONS - Simple manual save interface
 */

// Global persistence client instance
let globalJsonPersistence = null;

/**
 * Initialize JSON persistence for a page - MANUAL SAVE ONLY
 */
function initializeJsonPersistence(assessmentId, stepNumber) {
    globalJsonPersistence = new JsonPersistenceClient(assessmentId, stepNumber);
    globalJsonPersistence.initialize();
    
    console.log(`? Manual save system initialized for Assessment ${assessmentId}, Step ${stepNumber}`);
    return globalJsonPersistence;
}

/**
 * Manual save trigger
 */
async function saveFormDataToJson() {
    if (globalJsonPersistence) {
        return await globalJsonPersistence.manualSave();
    } else {
        console.warn('? JSON persistence not initialized. Call initializeJsonPersistence() first.');
        return { success: false, message: 'Not initialized' };
    }
}

/**
 * Save data when requested (no auto-save)
 */
function saveToJson(key, value) {
    console.log('?? saveToJson called - manual save system active (no auto-save)');
    // No automatic save - user must click Save Draft
}

/**
 * Load data placeholder (data loaded by server)
 */
function loadFromJson(key) {
    console.log('?? Data loaded from server-side JSON persistence');
    return null;
}

/**
 * Debug test function - can be called from browser console
 */
function debugTestManualSave() {
    console.log('?? DEBUG: Testing manual save system...');
    
    if (globalJsonPersistence) {
        console.log('?? DEBUG: globalJsonPersistence is available');
        
        // Test form data collection
        const testFormData = globalJsonPersistence.collectFormData();
        console.log('?? DEBUG: Collected form data:', testFormData);
        
        // Test manual save
        globalJsonPersistence.manualSave().then(result => {
            console.log('?? DEBUG: Manual save result:', result);
        }).catch(error => {
            console.error('?? DEBUG: Manual save error:', error);
        });
    } else {
        console.error('?? DEBUG: globalJsonPersistence is NOT available');
        console.log('?? DEBUG: Available global functions:', {
            initializeJsonPersistence: typeof window.initializeJsonPersistence,
            saveFormDataToJson: typeof window.saveFormDataToJson,
            globalJsonPersistence: typeof window.globalJsonPersistence
        });
    }
}

// Export for use in other scripts
window.JsonPersistenceClient = JsonPersistenceClient;
window.initializeJsonPersistence = initializeJsonPersistence;
window.saveFormDataToJson = saveFormDataToJson;
window.saveToJson = saveToJson;
window.loadFromJson = loadFromJson;
window.debugTestManualSave = debugTestManualSave;

console.log('? SIMPLIFIED JSON Persistence Client loaded - MANUAL SAVE ONLY');