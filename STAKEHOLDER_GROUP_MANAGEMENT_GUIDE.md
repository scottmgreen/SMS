# SMS Stakeholder Group Management - User Guide

## Overview
The SMS User Management system now includes comprehensive functionality for managing stakeholder group assignments. Users can assign or unassign individual stakeholder users to one or more groups using a modern, intuitive interface with full CQRS command integration.

## Features Implemented

### 1. **Group Assignment Modal**
- **Location**: System Administration > User Management > Stakeholder Users tab
- **Access**: Click the "Manage Groups" button (??) for any stakeholder user
- **Interface**: Dual-pane design with available groups on the left and assigned groups on the right

### 2. **Core Functionality**

#### **Assign Groups**
- Select one or more groups from the "Available Groups" panel
- Click the "Assign" button to assign selected groups to the user
- Groups are immediately moved from available to assigned panel

#### **Remove Groups**
- Select one or more groups from the "Assigned Groups" panel  
- Click the "Remove" button to remove selected groups from the user
- Groups are immediately moved back to the available panel

#### **Clear All Groups**
- Click the "Clear All" button to remove ALL group assignments from a user
- Includes confirmation dialog to prevent accidental clearing
- Useful for quickly resetting a user's group memberships

#### **Search & Filter**
- **Search**: Type in the search box to find groups by name, code, or description
- **Filter**: Use the status filter to show only active or inactive groups
- **Select All**: Checkbox to quickly select all visible groups in either panel

### 3. **CQRS Integration**
The system uses proper CQRS commands and queries:

#### **Commands Used**
- `AssignUserToStakeholderGroupCommand` - Assigns a user to a group
- `RemoveUserFromStakeholderGroupCommand` - Removes a user from a group  
- `ClearUserStakeholderGroupsCommand` - Clears all group memberships for a user

#### **Queries Used**
- `GetSMSStakeholderGroupsByUserCodeQuery` - Gets groups assigned to a user
- `GetAllSMSStakeholderGroupsQuery` - Gets all available groups

### 4. **API Endpoints**
All operations use AJAX calls to dedicated API endpoints:

- `OnGetUserGroupsAsync` - Loads current user group assignments
- `OnPostAssignGroupsToUserAsync` - Assigns multiple groups to a user
- `OnPostRemoveGroupsFromUserAsync` - Removes multiple groups from a user  
- `OnPostClearAllUserGroupsAsync` - Clears all groups from a user

## How to Use

### **Step 1: Navigate to User Management**
1. Go to **System Administration** from the main menu
2. Click **User Management**
3. Select the **Stakeholder Users** tab

### **Step 2: Open Group Management**
1. Find the stakeholder user you want to manage
2. Click the **Manage Groups** button (??) in the Actions column
3. The Group Management modal will open

### **Step 3: Manage Group Assignments**

#### **To Assign Groups:**
1. In the "Available Groups" panel (left), select the groups you want to assign
2. Use the search box or filter if needed to find specific groups
3. Click the **Assign** button (??)
4. Selected groups will move to the "Assigned Groups" panel

#### **To Remove Groups:**
1. In the "Assigned Groups" panel (right), select the groups you want to remove
2. Click the **Remove** button (??) 
3. Selected groups will move back to the "Available Groups" panel

#### **To Clear All Groups:**
1. Click the **Clear All** button
2. Confirm the action in the dialog
3. All groups will be removed from the user

### **Step 4: Verify Changes**
- Changes are saved immediately when you perform actions
- The modal shows real-time feedback with success/error messages
- Group counts are updated automatically
- Click **Refresh** to reload the current state if needed

## Technical Details

### **Data Flow**
1. **Frontend**: JavaScript handles UI interactions and AJAX calls
2. **Backend**: Razor Pages handlers process requests using MediatR
3. **Commands**: CQRS commands perform the actual data modifications
4. **Database**: Changes are persisted via stored procedures

### **Error Handling**
- Comprehensive error handling at all levels
- User-friendly error messages displayed in the modal
- Detailed logging for troubleshooting
- Graceful degradation if operations fail

### **Security**
- Anti-forgery tokens prevent CSRF attacks
- All operations require valid user authentication
- Proper input validation and sanitization
- Audit trail through command logging

## Benefits

1. **User-Friendly**: Intuitive drag-and-drop-style interface
2. **Efficient**: Batch operations for managing multiple groups at once
3. **Real-Time**: Immediate feedback and updates
4. **Robust**: Comprehensive error handling and validation
5. **Maintainable**: Clean separation of concerns using CQRS pattern
6. **Scalable**: Designed to handle large numbers of users and groups

## Troubleshooting

### **Common Issues**
- **Groups not loading**: Check network connectivity and server logs
- **Assignment failures**: Verify user permissions and group validity
- **Modal not opening**: Ensure JavaScript is enabled and page is fully loaded

### **Logging**
All operations are logged with detailed information for debugging:
- User actions and selections
- Command execution results  
- Error conditions and exceptions
- Performance metrics

---

**Note**: This system is designed for .NET 8 and follows modern web development best practices with proper separation of concerns, CQRS pattern implementation, and responsive UI design.