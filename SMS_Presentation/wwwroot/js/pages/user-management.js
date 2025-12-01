// User Management JavaScript functionality
(function() {
    'use strict';

    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function() {
        initializeUserManagement();
    });

    function initializeUserManagement() {
        // Tab switching
        initializeTabSwitching();
        
        // Search and filtering
        initializeSearchAndFilters();
        
        // Modal handlers
        initializeModalHandlers();
        
        // Group management
        initializeGroupManagement();
        
        // Action buttons
        initializeActionButtons();
    }

    function initializeTabSwitching() {
        const appUsersTab = document.getElementById('appUsersTab');
        const stakeholderUsersTab = document.getElementById('stakeholderUsersTab');
        const appUsersSection = document.getElementById('applicationUsersSection');
        const stakeholderUsersSection = document.getElementById('stakeholderUsersSection');

        if (appUsersTab && stakeholderUsersTab) {
            appUsersTab.addEventListener('click', function() {
                // Switch to application users
                appUsersTab.classList.add('active');
                stakeholderUsersTab.classList.remove('active');
                appUsersSection.style.display = 'block';
                stakeholderUsersSection.style.display = 'none';
            });

            stakeholderUsersTab.addEventListener('click', function() {
                // Switch to stakeholder users
                stakeholderUsersTab.classList.add('active');
                appUsersTab.classList.remove('active');
                appUsersSection.style.display = 'none';
                stakeholderUsersSection.style.display = 'block';
            });
        }
    }

    function initializeSearchAndFilters() {
        // Application users search
        const appUserSearch = document.getElementById('appUserSearch');
        if (appUserSearch) {
            appUserSearch.addEventListener('input', function() {
                filterTable('applicationUsersTable', this.value, [1, 2]); // Name and username columns
            });
        }

        // Application users role filter
        const appUserRoleFilter = document.getElementById('appUserRoleFilter');
        if (appUserRoleFilter) {
            appUserRoleFilter.addEventListener('change', function() {
                filterTableByColumn('applicationUsersTable', 3, this.value); // Role column
            });
        }

        // Application users status filter
        const appUserStatusFilter = document.getElementById('appUserStatusFilter');
        if (appUserStatusFilter) {
            appUserStatusFilter.addEventListener('change', function() {
                filterTableByStatus('applicationUsersTable', 5, this.value); // Status column
            });
        }

        // Stakeholder users search
        const stakeholderUserSearch = document.getElementById('stakeholderUserSearch');
        if (stakeholderUserSearch) {
            stakeholderUserSearch.addEventListener('input', function() {
                filterTable('stakeholderUsersTable', this.value, [1, 2]); // Name and username columns
            });
        }

        // Stakeholder type filter
        const stakeholderTypeFilter = document.getElementById('stakeholderTypeFilter');
        if (stakeholderTypeFilter) {
            stakeholderTypeFilter.addEventListener('change', function() {
                filterTableByColumn('stakeholderUsersTable', 3, this.value); // Type column
            });
        }

        // Stakeholder status filter
        const stakeholderStatusFilter = document.getElementById('stakeholderStatusFilter');
        if (stakeholderStatusFilter) {
            stakeholderStatusFilter.addEventListener('change', function() {
                filterTableByStatus('stakeholderUsersTable', 6, this.value); // Status column
            });
        }
    }

    function initializeModalHandlers() {
        // Edit application user modal
        const editAppUserButtons = document.querySelectorAll('.btn-edit-app-user');
        editAppUserButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                loadApplicationUserForEdit(userCode);
            });
        });

        // Edit stakeholder user modal
        const editStakeholderUserButtons = document.querySelectorAll('.btn-edit-stakeholder-user');
        editStakeholderUserButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                loadStakeholderUserForEdit(userCode);
            });
        });

        // View user details
        const viewAppUserButtons = document.querySelectorAll('.btn-view-app-user');
        viewAppUserButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                showUserDetails(userCode, 'application');
            });
        });

        const viewStakeholderUserButtons = document.querySelectorAll('.btn-view-stakeholder-user');
        viewStakeholderUserButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                showUserDetails(userCode, 'stakeholder');
            });
        });
    }

    function initializeGroupManagement() {
        // Manage groups modal
        const manageGroupsButtons = document.querySelectorAll('.btn-manage-groups');
        manageGroupsButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                loadUserGroups(userCode);
            });
        });

        // Group assignment buttons
        const assignButton = document.getElementById('assignSelectedGroups');
        if (assignButton) {
            assignButton.addEventListener('click', assignSelectedGroups);
        }

        const removeButton = document.getElementById('removeSelectedGroups');
        if (removeButton) {
            removeButton.addEventListener('click', removeSelectedGroups);
        }

        const clearButton = document.getElementById('clearAllGroups');
        if (clearButton) {
            clearButton.addEventListener('click', clearAllGroups);
        }
    }

    function initializeActionButtons() {
        // Activate/Deactivate buttons
        const activateAppButtons = document.querySelectorAll('.btn-activate-app-user');
        activateAppButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                toggleUserStatus(userCode, 'application', true);
            });
        });

        const deactivateAppButtons = document.querySelectorAll('.btn-deactivate-app-user');
        deactivateAppButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                toggleUserStatus(userCode, 'application', false);
            });
        });

        const activateStakeholderButtons = document.querySelectorAll('.btn-activate-stakeholder-user');
        activateStakeholderButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                toggleUserStatus(userCode, 'stakeholder', true);
            });
        });

        const deactivateStakeholderButtons = document.querySelectorAll('.btn-deactivate-stakeholder-user');
        deactivateStakeholderButtons.forEach(button => {
            button.addEventListener('click', function() {
                const userCode = this.getAttribute('data-user-code');
                toggleUserStatus(userCode, 'stakeholder', false);
            });
        });
    }

    // Utility functions
    function filterTable(tableId, searchTerm, columnIndices) {
        const table = document.getElementById(tableId);
        if (!table) return;

        const tbody = table.querySelector('tbody');
        const rows = tbody.querySelectorAll('tr');

        rows.forEach(row => {
            let found = false;
            columnIndices.forEach(colIndex => {
                const cell = row.cells[colIndex];
                if (cell && cell.textContent.toLowerCase().includes(searchTerm.toLowerCase())) {
                    found = true;
                }
            });
            row.style.display = found ? '' : 'none';
        });
    }

    function filterTableByColumn(tableId, columnIndex, filterValue) {
        const table = document.getElementById(tableId);
        if (!table) return;

        const tbody = table.querySelector('tbody');
        const rows = tbody.querySelectorAll('tr');

        rows.forEach(row => {
            const cell = row.cells[columnIndex];
            if (!cell) return;

            if (!filterValue || cell.textContent.includes(filterValue)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    }

    function filterTableByStatus(tableId, columnIndex, filterValue) {
        const table = document.getElementById(tableId);
        if (!table) return;

        const tbody = table.querySelector('tbody');
        const rows = tbody.querySelectorAll('tr');

        rows.forEach(row => {
            const cell = row.cells[columnIndex];
            if (!cell) return;

            const isActive = cell.textContent.includes('Active');
            
            if (!filterValue || 
                (filterValue === 'true' && isActive) || 
                (filterValue === 'false' && !isActive)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    }

    function loadApplicationUserForEdit(userCode) {
        // Find user data from the table
        const row = document.querySelector(`tr[data-user-code="${userCode}"]`);
        if (!row) return;

        const cells = row.cells;
        const fullName = cells[1].textContent.trim();
        const nameParts = fullName.split(' ');
        const firstName = nameParts[0] || '';
        const lastName = nameParts.slice(1).join(' ') || '';
        const role = cells[3].textContent.trim();
        const permissionLevel = cells[4].textContent.trim();

        // Populate edit modal
        document.getElementById('editUserId').value = userCode;
        document.getElementById('editFirstName').value = firstName;
        document.getElementById('editLastName').value = lastName;
        document.getElementById('editApplicationRole').value = role;
        document.getElementById('editPermissionLevel').value = permissionLevel;
    }

    function loadStakeholderUserForEdit(userCode) {
        // Find user data from the table
        const row = document.querySelector(`tr[data-user-code="${userCode}"]`);
        if (!row) return;

        const cells = row.cells;
        const fullName = cells[1].textContent.trim();
        const nameParts = fullName.split(' ');
        const firstName = nameParts[0] || '';
        const lastName = nameParts.slice(1).join(' ') || '';
        const type = cells[3].textContent.trim();
        const organization = cells[4].textContent.trim();
        const accessLevel = cells[5].textContent.trim();

        // Populate edit modal
        document.getElementById('editStakeholderUserId').value = userCode;
        document.getElementById('editStakeholderFirstName').value = firstName;
        document.getElementById('editStakeholderLastName').value = lastName;
        document.getElementById('editStakeholderType').value = type;
        document.getElementById('editOrganization').value = organization;
        document.getElementById('editAccessLevel').value = accessLevel;
    }

    function showUserDetails(userCode, userType) {
        // This would typically make an AJAX call to get full user details
        // For now, we'll show basic information from the table
        const row = document.querySelector(`tr[data-user-code="${userCode}"]`);
        if (!row) return;

        const modal = new bootstrap.Modal(document.getElementById('userDetailsModal'));
        const content = document.getElementById('userDetailsContent');
        
        // Create details HTML based on user type
        let detailsHtml = `<div class="row">`;
        // Add user details based on table data
        detailsHtml += `</div>`;
        
        content.innerHTML = detailsHtml;
        modal.show();
    }

    function loadUserGroups(userCode) {
        document.getElementById('manageGroupsUserId').value = userCode;
        // This would typically load current user groups via AJAX
        // For now, clear selections
        document.querySelectorAll('.available-group').forEach(checkbox => {
            checkbox.checked = false;
        });
    }

    function assignSelectedGroups() {
        const userCode = document.getElementById('manageGroupsUserId').value;
        const selectedGroups = Array.from(document.querySelectorAll('.available-group:checked'))
            .map(cb => cb.value);

        selectedGroups.forEach(groupCode => {
            // Make POST request to assign user to group
            fetch(window.location.pathname, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: `handler=AssignUserToGroup&userCode=${userCode}&groupCode=${groupCode}`
            }).then(() => {
                // Refresh page or update UI
                location.reload();
            });
        });
    }

    function removeSelectedGroups() {
        const userCode = document.getElementById('manageGroupsUserId').value;
        // Similar implementation for removing groups
    }

    function clearAllGroups() {
        const userCode = document.getElementById('manageGroupsUserId').value;
        if (confirm('Are you sure you want to clear all group memberships for this user?')) {
            fetch(window.location.pathname, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: `handler=ClearUserGroups&userCode=${userCode}`
            }).then(() => {
                location.reload();
            });
        }
    }

    function toggleUserStatus(userCode, userType, activate) {
        const action = activate ? 'Activate' : 'Deactivate';
        const handler = userType === 'application' ? 
            (activate ? 'ActivateApplicationUser' : 'DeactivateApplicationUser') :
            (activate ? 'ActivateStakeholderUser' : 'DeactivateStakeholderUser');

        if (confirm(`Are you sure you want to ${action.toLowerCase()} this user?`)) {
            fetch(window.location.pathname, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: `handler=${handler}&userId=${userCode}`
            }).then(() => {
                location.reload();
            });
        }
    }

})();