/**
 * ?? COMPREHENSIVE STAKEHOLDER MANAGEMENT FOR RISK ASSESSMENT WIZARD
 * 
 * This script provides complete stakeholder group and individual stakeholder selection
 * for the SMS Risk Assessment Step 1 (System Description).
 * 
 * Features:
 * - Stakeholder group selection with real-time updates
 * - Individual stakeholder selection
 * - Dynamic display updates
 * - Form data preservation for submission
 * - Integration with comprehensive data persistence
 */

// ?? GLOBAL STAKEHOLDER STATE
let selectedStakeholderGroups = [];
let selectedIndividualStakeholders = [];

/**
 * ?? Initialize comprehensive stakeholder management
 */
function initializeStakeholderManagement() {
    console.log('?? Initializing comprehensive stakeholder management...');
    
    // Initialize stakeholder group checkboxes
    initializeStakeholderGroupCheckboxes();
    
    // Initialize individual stakeholder selection
    initializeIndividualStakeholderSelection();
    
    // Load any existing selections
    loadExistingStakeholderSelections();
    
    // Update display
    updateStakeholderDisplay();
    
    console.log('? Stakeholder management initialized successfully');
}

/**
 * ?? Initialize stakeholder group checkboxes
 */
function initializeStakeholderGroupCheckboxes() {
    const groupCheckboxes = document.querySelectorAll('.stakeholder-group-checkbox');
    console.log(`?? Found ${groupCheckboxes.length} stakeholder group checkboxes`);
    
    groupCheckboxes.forEach(checkbox => {
        // Remove any existing listeners to prevent duplicates
        checkbox.removeEventListener('change', handleStakeholderGroupChange);
        
        // Add new listener
        checkbox.addEventListener('change', handleStakeholderGroupChange);
        
        // Check if this group is already selected
        if (selectedStakeholderGroups.includes(checkbox.value)) {
            checkbox.checked = true;
        }
    });
}

/**
 * ?? Handle stakeholder group checkbox changes
 */
function handleStakeholderGroupChange(event) {
    const checkbox = event.target;
    const groupName = checkbox.value;
    const isChecked = checkbox.checked;
    
    console.log(`?? Stakeholder group changed: ${groupName} = ${isChecked}`);
    
    if (isChecked) {
        // Add to selected groups if not already present
        if (!selectedStakeholderGroups.includes(groupName)) {
            selectedStakeholderGroups.push(groupName);
        }
    } else {
        // Remove from selected groups
        selectedStakeholderGroups = selectedStakeholderGroups.filter(g => g !== groupName);
    }
    
    // Update displays and form data
    updateStakeholderDisplay();
    updateStakeholderFormData();
    
    console.log(`? Selected stakeholder groups:`, selectedStakeholderGroups);
}

/**
 * ?? Initialize individual stakeholder selection
 */
function initializeIndividualStakeholderSelection() {
    const individualSelect = document.getElementById('individualStakeholderSelect');
    if (!individualSelect) {
        console.log('?? Individual stakeholder select not found');
        return;
    }
    
    // Remove any existing listeners
    individualSelect.removeEventListener('change', handleIndividualStakeholderChange);
    
    // Add new listener
    individualSelect.addEventListener('change', handleIndividualStakeholderChange);
    
    console.log(`?? Individual stakeholder select initialized with ${individualSelect.options.length} options`);
}

/**
 * ?? Handle individual stakeholder selection changes
 */
function handleIndividualStakeholderChange(event) {
    const select = event.target;
    const selectedOptions = Array.from(select.selectedOptions);
    
    selectedIndividualStakeholders = selectedOptions.map(option => ({
        id: option.value,
        name: option.dataset.name || option.text,
        organization: option.dataset.organization || '',
        type: option.dataset.type || '',
        category: option.dataset.category || ''
    }));
    
    console.log(`?? Individual stakeholders selected:`, selectedIndividualStakeholders);
    
    // Update displays and form data
    updateStakeholderDisplay();
    updateStakeholderFormData();
}

/**
 * ?? Update stakeholder display containers
 */
function updateStakeholderDisplay() {
    updateSelectedStakeholdersContainer();
    updateStakeholderSummary();
}

/**
 * ?? Update the selected stakeholders display container
 */
function updateSelectedStakeholdersContainer() {
    const container = document.getElementById('selectedStakeholdersContainer');
    if (!container) {
        console.log('?? Selected stakeholders container not found');
        return;
    }
    
    let html = '';
    
    // Add selected groups
    if (selectedStakeholderGroups.length > 0) {
        html += '<div class="mb-2">';
        html += '<h6 class="text-primary mb-1"><i class="fas fa-users"></i> Selected Groups:</h6>';
        
        selectedStakeholderGroups.forEach(groupName => {
            html += `<span class="badge bg-primary me-1 mb-1">${groupName}</span>`;
        });
        
        html += '</div>';
    }
    
    // Add individual stakeholders
    if (selectedIndividualStakeholders.length > 0) {
        html += '<div class="mb-2">';
        html += '<h6 class="text-info mb-1"><i class="fas fa-user"></i> Individual Stakeholders:</h6>';
        
        selectedIndividualStakeholders.forEach(stakeholder => {
            html += `
                <div class="badge bg-info me-1 mb-1">
                    ${stakeholder.name}
                    ${stakeholder.organization ? ` (${stakeholder.organization})` : ''}
                </div>
            `;
        });
        
        html += '</div>';
    }
    
    // Show message if no selections
    if (selectedStakeholderGroups.length === 0 && selectedIndividualStakeholders.length === 0) {
        html = '<small class="text-muted"><i class="fas fa-info-circle"></i> No stakeholders selected yet. Select groups or individuals above.</small>';
    }
    
    container.innerHTML = html;
}

/**
 * ?? Update stakeholder summary for display
 */
function updateStakeholderSummary() {
    // This could be used for additional summary displays if needed
    const totalSelected = selectedStakeholderGroups.length + selectedIndividualStakeholders.length;
    console.log(`?? Stakeholder summary: ${totalSelected} total selections (${selectedStakeholderGroups.length} groups, ${selectedIndividualStakeholders.length} individuals)`);
}

/**
 * ?? Update hidden form data for submission
 */
function updateStakeholderFormData() {
    // Update the main stakeholder groups hidden field
    const groupsHiddenField = document.getElementById('stakeholderGroupsHidden');
    if (groupsHiddenField) {
        groupsHiddenField.value = selectedStakeholderGroups.join(', ');
        console.log(`?? Updated stakeholder groups form data: ${groupsHiddenField.value}`);
    }
    
    // Create or update individual stakeholders hidden field
    let individualsHiddenField = document.getElementById('selectedIndividualStakeholdersHidden');
    if (!individualsHiddenField) {
        individualsHiddenField = document.createElement('input');
        individualsHiddenField.type = 'hidden';
        individualsHiddenField.id = 'selectedIndividualStakeholdersHidden';
        individualsHiddenField.name = 'SelectedIndividualStakeholders';
        
        const form = document.querySelector('form');
        if (form) {
            form.appendChild(individualsHiddenField);
        }
    }
    
    if (individualsHiddenField) {
        individualsHiddenField.value = JSON.stringify(selectedIndividualStakeholders);
        console.log(`?? Updated individual stakeholders form data: ${selectedIndividualStakeholders.length} stakeholders`);
    }
}

/**
 * ?? Load existing stakeholder selections (from server data or saved state)
 */
function loadExistingStakeholderSelections() {
    // Load stakeholder groups from hidden field
    const groupsHiddenField = document.getElementById('stakeholderGroupsHidden');
    if (groupsHiddenField && groupsHiddenField.value) {
        const groupNames = groupsHiddenField.value.split(',').map(g => g.trim()).filter(g => g);
        selectedStakeholderGroups = [...groupNames];
        
        // Check the corresponding checkboxes
        groupNames.forEach(groupName => {
            const checkbox = document.querySelector(`input[type="checkbox"][value="${groupName}"]`);
            if (checkbox) {
                checkbox.checked = true;
            }
        });
        
        console.log(`?? Loaded existing stakeholder groups:`, selectedStakeholderGroups);
    }
    
    // Load individual stakeholders from hidden field
    const individualsHiddenField = document.getElementById('selectedIndividualStakeholdersHidden');
    if (individualsHiddenField && individualsHiddenField.value) {
        try {
            selectedIndividualStakeholders = JSON.parse(individualsHiddenField.value) || [];
            
            // Select the corresponding options in the multi-select
            const individualSelect = document.getElementById('individualStakeholderSelect');
            if (individualSelect) {
                Array.from(individualSelect.options).forEach(option => {
                    const isSelected = selectedIndividualStakeholders.some(s => s.id === option.value);
                    option.selected = isSelected;
                });
            }
            
            console.log(`?? Loaded existing individual stakeholders:`, selectedIndividualStakeholders);
        } catch (error) {
            console.error('? Error loading individual stakeholders:', error);
            selectedIndividualStakeholders = [];
        }
    }
}

/**
 * ?? Clear all stakeholder selections
 */
function clearAllStakeholderSelections() {
    console.log('?? Clearing all stakeholder selections...');
    
    // Clear arrays
    selectedStakeholderGroups = [];
    selectedIndividualStakeholders = [];
    
    // Clear group checkboxes
    const groupCheckboxes = document.querySelectorAll('.stakeholder-group-checkbox');
    groupCheckboxes.forEach(checkbox => {
        checkbox.checked = false;
    });
    
    // Clear individual select
    const individualSelect = document.getElementById('individualStakeholderSelect');
    if (individualSelect) {
        Array.from(individualSelect.options).forEach(option => {
            option.selected = false;
        });
    }
    
    // Update displays and form data
    updateStakeholderDisplay();
    updateStakeholderFormData();
    
    console.log('? All stakeholder selections cleared');
}

/**
 * ?? Get stakeholder selection statistics
 */
function getStakeholderStats() {
    return {
        groupCount: selectedStakeholderGroups.length,
        individualCount: selectedIndividualStakeholders.length,
        totalSelections: selectedStakeholderGroups.length + selectedIndividualStakeholders.length,
        groups: [...selectedStakeholderGroups],
        individuals: [...selectedIndividualStakeholders]
    };
}

/**
 * ?? BACKWARD COMPATIBILITY FUNCTIONS
 * These functions maintain compatibility with existing code
 */

// Legacy function for toggling stakeholder groups
function toggleStakeholderGroup(groupName, isChecked) {
    console.log(`?? Legacy toggleStakeholderGroup called: ${groupName} = ${isChecked}`);
    
    if (isChecked) {
        if (!selectedStakeholderGroups.includes(groupName)) {
            selectedStakeholderGroups.push(groupName);
        }
    } else {
        selectedStakeholderGroups = selectedStakeholderGroups.filter(g => g !== groupName);
    }
    
    updateStakeholderDisplay();
    updateStakeholderFormData();
    
    console.log(`? Updated stakeholder groups:`, selectedStakeholderGroups);
}

// Legacy function for updating selected stakeholders
function updateSelectedStakeholders() {
    console.log('?? Legacy updateSelectedStakeholders called');
    
    // Get the individual select element
    const individualSelect = document.getElementById('individualStakeholderSelect');
    if (individualSelect) {
        // Manually trigger change event to update selections
        const event = new Event('change');
        individualSelect.dispatchEvent(event);
    }
    
    // Re-initialize to pick up any changes
    initializeStakeholderGroupCheckboxes();
    loadExistingStakeholderSelections();
    updateStakeholderDisplay();
    updateStakeholderFormData();
}

/**
 * ?? MODAL INTEGRATION
 * Handle stakeholder creation via modal
 */

// Handle successful stakeholder creation from modal
function handleStakeholderCreated(newStakeholder) {
    console.log('?? New stakeholder created:', newStakeholder);
    
    // Refresh the stakeholder lists
    // This would typically reload the page or refresh the stakeholder data
    // For now, we'll just log it and the page refresh will pick up the new stakeholder
    
    console.log('? Stakeholder creation handled - page refresh recommended');
}

// Initialize stakeholder management when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Small delay to ensure all elements are loaded
    setTimeout(() => {
        if (typeof initializeStakeholderManagement === 'function') {
            initializeStakeholderManagement();
        }
    }, 100);
});

console.log('?? Comprehensive stakeholder management script loaded');

// Export functions for global access
window.initializeStakeholderManagement = initializeStakeholderManagement;
window.toggleStakeholderGroup = toggleStakeholderGroup;
window.updateSelectedStakeholders = updateSelectedStakeholders;
window.clearAllStakeholderSelections = clearAllStakeholderSelections;
window.getStakeholderStats = getStakeholderStats;