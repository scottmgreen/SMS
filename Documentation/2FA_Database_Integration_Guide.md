# Two-Factor Authentication Database Integration Guide

## Overview
This document outlines all the database and repository changes needed to support Two-Factor Authentication (2FA) in your SMS application.

## Database Schema Changes Required

### Step 1: Add 2FA Columns to User Tables

```sql
-- Add columns to tbld_SMSApplicationUsers
ALTER TABLE [dbo].[tbld_SMSApplicationUsers] ADD 
    [fldv_TwoFactorSecretKey] NVARCHAR(255) NULL,
    [fldb_TwoFactorEnabled] BIT NOT NULL DEFAULT 0,
    [fldv_BackupCodes] NVARCHAR(MAX) NULL,
    [fldd_TwoFactorSetupDate] DATETIME2(7) NULL,
    [fldi_FailedTwoFactorAttempts] INT NOT NULL DEFAULT 0,
    [fldd_TwoFactorLockedUntil] DATETIME2(7) NULL;

-- Add columns to tbld_SMSOrganizationalUsers
ALTER TABLE [dbo].[tbld_SMSOrganizationalUsers] ADD 
    [fldv_TwoFactorSecretKey] NVARCHAR(255) NULL,
    [fldb_TwoFactorEnabled] BIT NOT NULL DEFAULT 0,
    [fldv_BackupCodes] NVARCHAR(MAX) NULL,
    [fldd_TwoFactorSetupDate] DATETIME2(7) NULL,
    [fldi_FailedTwoFactorAttempts] INT NOT NULL DEFAULT 0,
    [fldd_TwoFactorLockedUntil] DATETIME2(7) NULL;

-- Add columns to tbld_SMSStakeholderUsers
ALTER TABLE [dbo].[tbld_SMSStakeholderUsers] ADD 
    [fldv_TwoFactorSecretKey] NVARCHAR(255) NULL,
    [fldb_TwoFactorEnabled] BIT NOT NULL DEFAULT 0,
    [fldv_BackupCodes] NVARCHAR(MAX) NULL,
    [fldd_TwoFactorSetupDate] DATETIME2(7) NULL,
    [fldi_FailedTwoFactorAttempts] INT NOT NULL DEFAULT 0,
    [fldd_TwoFactorLockedUntil] DATETIME2(7) NULL;
```

### Step 2: Update Existing Stored Procedures

Your existing `pr_SMSApplicationUser_Update` needs to be updated with optional 2FA parameters:

```sql
-- Add these parameters to pr_SMSApplicationUser_Update
@pTwoFactorSecretKey NVARCHAR(255) = NULL,
@pTwoFactorEnabled BIT = NULL,
@pBackupCodes NVARCHAR(MAX) = NULL,
@pTwoFactorSetupDate DATETIME2(7) = NULL,
@pFailedTwoFactorAttempts INT = NULL,
@pTwoFactorLockedUntil DATETIME2(7) = NULL
```

### Step 3: Create Specialized 2FA Stored Procedures

Run the SQL scripts I've provided:
- `Database/StoredProcedures/pr_SMSApplicationUser_2FA_Specialized.sql`
- `Database/StoredProcedures/pr_SMSOrganizationalUser_2FA.sql`
- `Database/StoredProcedures/pr_SMSStakeholderUser_2FA.sql`

## Code Changes Completed

### ? Domain Layer
- Added 2FA properties to `BaseUser` entity
- All user types inherit these properties

### ? Infrastructure Layer
- Updated `FieldNames.cs` with 2FA field constants
- Updated `Mappers.cs` to map 2FA properties from database
- Added 2FA repository methods to `SMSApplicationUserRepository`
- Updated `ISMSApplicationUserRepository` interface

### ? Application Layer
- Enhanced `TwoFactorAuthService` with debugging methods
- Ready for integration with login flow

## 2FA Flow During Login

### When User Attempts 2FA Verification:

1. **Success Path:**
   ```csharp
   // Reset failed attempts on successful verification
   await repository.Reset2FAFailedAttemptsAsync(userCode);
   ```

2. **Failure Path:**
   ```csharp
   // Increment failed attempts
   var failedAttempts = user.FailedTwoFactorAttempts + 1;
   DateTime? lockoutUntil = null;
   
   if (failedAttempts >= 5) // Configurable threshold
   {
       lockoutUntil = DateTime.UtcNow.AddMinutes(15); // Lockout for 15 minutes
   }
   
   await repository.Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil);
   ```

3. **First-Time Setup:**
   ```csharp
   // When user first sets up 2FA
   var secretKey = twoFactorService.GenerateSecretKey();
   var backupCodes = twoFactorService.GenerateBackupCodes(); // Optional
   
   await repository.Setup2FAAsync(userCode, secretKey, backupCodes);
   ```

## Repository Methods Available

### SMSApplicationUserRepository (and similar for other user types):

- `Setup2FAAsync(userCode, secretKey, backupCodes, updatedBy)`
- `Update2FAFailedAttemptsAsync(userCode, failedAttempts, lockoutUntil, updatedBy)`
- `Reset2FAFailedAttemptsAsync(userCode, updatedBy)`
- `Disable2FAAsync(userCode, updatedBy)`

## Integration with Login Flow

### Updated Login Process:
1. User enters credentials ? Authenticate user
2. If user has 2FA enabled ? Show 2FA verification page
3. User enters 6-digit code ? Validate with stored secret key
4. On success ? Reset failed attempts, complete login
5. On failure ? Increment failed attempts, optionally lockout

### Example Integration in Login.razor.cs:
```csharp
if (authResult.IsSuccess && authResult.User.TwoFactorEnabled)
{
    // Store pending user for 2FA verification
    // Navigate to 2FA verification page
    Navigation.NavigateTo($"/verify-2fa?user={userCode}");
}
```

## Security Considerations

### 2FA Field Security:
- **TwoFactorSecretKey**: Encrypted storage recommended
- **BackupCodes**: JSON array of one-time use codes
- **FailedAttempts**: Prevents brute force attacks
- **LockedUntil**: Temporary lockout mechanism

### Database Indexes (Recommended):
```sql
-- Index for 2FA lookups
CREATE INDEX IX_SMSApplicationUsers_2FA_Enabled 
ON tbld_SMSApplicationUsers (fldb_TwoFactorEnabled, fldv_Code)
WHERE fldb_TwoFactorEnabled = 1;

-- Index for lockout checks
CREATE INDEX IX_SMSApplicationUsers_2FA_Lockout 
ON tbld_SMSApplicationUsers (fldd_TwoFactorLockedUntil, fldv_Code)
WHERE fldd_TwoFactorLockedUntil IS NOT NULL;
```

## Testing the Implementation

### Test Cases:
1. **First-time 2FA setup** - Generate QR code, verify setup
2. **Successful 2FA verification** - Correct code acceptance
3. **Failed 2FA verification** - Increment failed attempts
4. **2FA lockout** - Account lockout after X failed attempts
5. **2FA unlock** - Automatic unlock after timeout
6. **Disable 2FA** - Remove 2FA requirement

## Next Steps

1. **Run database schema updates** (add columns)
2. **Deploy stored procedures** (run SQL scripts)
3. **Test repository methods** (unit tests)
4. **Integrate with login flow** (update Login.razor.cs)
5. **Create 2FA setup UI** (user management pages)

The infrastructure is now ready for full 2FA integration! ??