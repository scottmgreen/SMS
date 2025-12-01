# Testing Guide for Stakeholder Group Management

## Step-by-Step Testing Process

### Phase 1: Setup Test Data
1. **Navigate to User Management**: Go to System Administration ? User Management
2. **Check Current State**:
   - Go to "Stakeholder Groups" tab - should show "No stakeholder groups found"
   - Go to "Stakeholder Users" tab - check if any users exist
3. **Create Default Groups**: 
   - In the Stakeholder Groups tab, click "Create Default Groups" button
   - Confirm the action
   - Page should reload and show 6 new groups

### Phase 2: Test Group Management
1. **Open Group Management Modal**:
   - Go to "Stakeholder Users" tab
   - Find any stakeholder user
   - Click either:
     - The "Groups" button in the Groups column, OR
     - The groups icon (??) button in the Actions column
2. **Verify Modal Functionality**:
   - Modal should open with user name and code displayed
   - Left panel should show available groups (the ones you just created)
   - Right panel should show "No groups assigned"
   - Console should show debug messages (open browser dev tools F12)

### Phase 3: Test Group Assignment
1. **Assign Groups**:
   - Select one or more groups from the left panel (Available Groups)
   - Click the "Assign" button (??)
   - Groups should move from left to right panel
   - Success message should appear
2. **Remove Groups**:
   - Select groups from the right panel (Assigned Groups)
   - Click the "Remove" button (??)
   - Groups should move from right to left panel
3. **Test Clear All**:
   - Click "Clear All" button
   - Confirm the action
   - All groups should be removed from the user

### Phase 4: Verify Persistence
1. **Close and Reopen Modal**:
   - Close the modal
   - Reopen it for the same user
   - Assigned groups should be remembered
2. **Test Different Users**:
   - Try the same process with different stakeholder users
   - Each should have independent group assignments

### Troubleshooting
If something doesn't work:

1. **Check Browser Console (F12)**:
   - Look for JavaScript errors
   - Check the debug messages we added
2. **Check Network Tab**:
   - Look for failed API calls
   - Check response status codes
3. **Common Issues**:
   - **Groups not loading**: Check if default groups were created successfully
   - **Modal not opening**: Check for JavaScript errors
   - **API calls failing**: Check browser network tab for error details
   - **No users showing**: Need to create stakeholder users first

### Success Criteria
? Modal opens when buttons are clicked
? Groups are loaded and displayed in both panels
? Groups can be assigned and removed
? Changes are persisted between modal opens
? Multiple users can have different group assignments
? Clear All functionality works
? Search and filter work in the modal

### If Everything Works:
The functionality is complete! Users can now:
- Assign stakeholder users to multiple groups
- Remove users from groups
- Manage group memberships through an intuitive interface
- All changes are saved using proper CQRS commands
- Full audit trail is maintained