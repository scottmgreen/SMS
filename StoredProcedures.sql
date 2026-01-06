# Safety Performance Indicator (SPI) Stored Procedures

## Table Structure

First, create the tables if they don't exist:

```sql
-- Main SPI table - Following established field naming convention
CREATE TABLE tbld_SafetyPerformanceIndicators (
    fldi_ID INT IDENTITY(1,1) PRIMARY KEY,
    fldv_Code NVARCHAR(50) NOT NULL UNIQUE,
    fldv_Name NVARCHAR(255) NOT NULL,
    fldc_Description NVARCHAR(MAX) NULL,
    fldv_IndicatorType NVARCHAR(100) NOT NULL,
    fldv_Status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    fldv_MeasurementUnit NVARCHAR(100) NULL,
    fldv_MeasurementFrequency NVARCHAR(50) NOT NULL DEFAULT 'MONTHLY',
    fldc_CalculationMethod NVARCHAR(MAX) NULL,
    fldv_DataSource NVARCHAR(255) NULL,
    fldm_TargetValue DECIMAL(18,4) NULL,
    fldm_AcceptableRange DECIMAL(18,4) NULL,
    fldm_WarningThreshold DECIMAL(18,4) NULL,
    fldm_CriticalThreshold DECIMAL(18,4) NULL,
    fldv_ResponsibleDepartment NVARCHAR(255) NULL,
    fldv_DataOwner NVARCHAR(255) NULL,
    fldv_ReviewAuthority NVARCHAR(255) NULL,
    fldd_NextReviewDate DATETIME2 NULL,
    fldd_LastReviewDate DATETIME2 NULL,
    fldc_LastReviewNotes NVARCHAR(MAX) NULL,
    fldb_AlertsEnabled BIT NOT NULL DEFAULT 1,
    fldc_AlertRecipients NVARCHAR(MAX) NULL,
    fldv_CreatedBy NVARCHAR(255) NOT NULL,
    fldd_CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    fldv_UpdatedBy NVARCHAR(255) NULL,
    fldd_UpdatedDate DATETIME2 NULL
);

-- SPI Data Points table - Following established field naming convention
CREATE TABLE tbld_SPIDataPoints (
    fldi_ID INT IDENTITY(1,1) PRIMARY KEY,
    fldi_SPIId INT NOT NULL FOREIGN KEY REFERENCES tbld_SafetyPerformanceIndicators(fldi_ID),
    fldm_Value DECIMAL(18,4) NOT NULL,
    fldd_MeasurementDate DATETIME2 NOT NULL,
    fldv_Period NVARCHAR(50) NOT NULL,
    fldv_DataSource NVARCHAR(255) NULL,
    fldv_EnteredBy NVARCHAR(255) NOT NULL,
    fldd_EnteredDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    fldc_Notes NVARCHAR(MAX) NULL,
    fldb_IsVerified BIT NOT NULL DEFAULT 0,
    fldv_VerifiedBy NVARCHAR(255) NULL,
    fldd_VerifiedDate DATETIME2 NULL
);
```

## Core CRUD Operations

### 1. Insert SPI

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_Insert
(
    @pCode NVARCHAR(50),
    @pName NVARCHAR(255),
    @pDescription NVARCHAR(MAX),
    @pIndicatorType NVARCHAR(100),
    @pStatus NVARCHAR(50),
    @pMeasurementUnit NVARCHAR(100),
    @pMeasurementFrequency NVARCHAR(50),
    @pCalculationMethod NVARCHAR(MAX),
    @pDataSource NVARCHAR(255),
    @pTargetValue DECIMAL(18,4) = NULL,
    @pAcceptableRange DECIMAL(18,4) = NULL,
    @pWarningThreshold DECIMAL(18,4) = NULL,
    @pCriticalThreshold DECIMAL(18,4) = NULL,
    @pResponsibleDepartment NVARCHAR(255),
    @pDataOwner NVARCHAR(255),
    @pReviewAuthority NVARCHAR(255),
    @pNextReviewDate DATETIME2 = NULL,
    @pLastReviewDate DATETIME2 = NULL,
    @pLastReviewNotes NVARCHAR(MAX) = NULL,
    @pAlertsEnabled BIT,
    @pAlertRecipients NVARCHAR(MAX) = NULL,
    @pCreatedBy NVARCHAR(255),
    @pCreatedDate DATETIME2,
    @pNewID INT OUTPUT,
    @pNewSPICode NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Generate code if not provided
        IF @pCode IS NULL OR @pCode = ''
        BEGIN
            DECLARE @NextNumber INT;
            SELECT @NextNumber = ISNULL(MAX(CAST(SUBSTRING(fldv_Code, 5, 10) AS INT)), 0) + 1
            FROM tbld_SafetyPerformanceIndicators
            WHERE fldv_Code LIKE 'SPI-%';
            
            SET @pCode = 'SPI-' + RIGHT('000000' + CAST(@NextNumber AS VARCHAR), 6);
        END
        
        INSERT INTO tbld_SafetyPerformanceIndicators (
            fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status, 
            fldv_MeasurementUnit, fldv_MeasurementFrequency, fldc_CalculationMethod, fldv_DataSource,
            fldm_TargetValue, fldm_AcceptableRange, fldm_WarningThreshold, fldm_CriticalThreshold,
            fldv_ResponsibleDepartment, fldv_DataOwner, fldv_ReviewAuthority,
            fldd_NextReviewDate, fldd_LastReviewDate, fldc_LastReviewNotes,
            fldb_AlertsEnabled, fldc_AlertRecipients, fldv_CreatedBy, fldd_CreatedDate
        )
        VALUES (
            @pCode, @pName, @pDescription, @pIndicatorType, @pStatus,
            @pMeasurementUnit, @pMeasurementFrequency, @pCalculationMethod, @pDataSource,
            @pTargetValue, @pAcceptableRange, @pWarningThreshold, @pCriticalThreshold,
            @pResponsibleDepartment, @pDataOwner, @pReviewAuthority,
            @pNextReviewDate, @pLastReviewDate, @pLastReviewNotes,
            @pAlertsEnabled, @pAlertRecipients, @pCreatedBy, @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @pNewSPICode = @pCode;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        THROW;
    END CATCH
END;
```

### 2. Update SPI

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_Update
(
    @pID NVARCHAR(50),
    @pCode NVARCHAR(50),
    @pName NVARCHAR(255),
    @pDescription NVARCHAR(MAX),
    @pIndicatorType NVARCHAR(100),
    @pStatus NVARCHAR(50),
    @pMeasurementUnit NVARCHAR(100),
    @pMeasurementFrequency NVARCHAR(50),
    @pCalculationMethod NVARCHAR(MAX),
    @pDataSource NVARCHAR(255),
    @pTargetValue DECIMAL(18,4) = NULL,
    @pAcceptableRange DECIMAL(18,4) = NULL,
    @pWarningThreshold DECIMAL(18,4) = NULL,
    @pCriticalThreshold DECIMAL(18,4) = NULL,
    @pResponsibleDepartment NVARCHAR(255),
    @pDataOwner NVARCHAR(255),
    @pReviewAuthority NVARCHAR(255),
    @pNextReviewDate DATETIME2 = NULL,
    @pLastReviewDate DATETIME2 = NULL,
    @pLastReviewNotes NVARCHAR(MAX) = NULL,
    @pAlertsEnabled BIT,
    @pAlertRecipients NVARCHAR(MAX) = NULL,
    @pUpdatedBy NVARCHAR(255),
    @pUpdatedDate DATETIME2
)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE tbld_SafetyPerformanceIndicators
        SET 
            fldv_Code = @pCode,
            fldv_Name = @pName,
            fldc_Description = @pDescription,
            fldv_IndicatorType = @pIndicatorType,
            fldv_Status = @pStatus,
            fldv_MeasurementUnit = @pMeasurementUnit,
            fldv_MeasurementFrequency = @pMeasurementFrequency,
            fldc_CalculationMethod = @pCalculationMethod,
            fldv_DataSource = @pDataSource,
            fldm_TargetValue = @pTargetValue,
            fldm_AcceptableRange = @pAcceptableRange,
            fldm_WarningThreshold = @pWarningThreshold,
            fldm_CriticalThreshold = @pCriticalThreshold,
            fldv_ResponsibleDepartment = @pResponsibleDepartment,
            fldv_DataOwner = @pDataOwner,
            fldv_ReviewAuthority = @pReviewAuthority,
            fldd_NextReviewDate = @pNextReviewDate,
            fldd_LastReviewDate = @pLastReviewDate,
            fldc_LastReviewNotes = @pLastReviewNotes,
            fldb_AlertsEnabled = @pAlertsEnabled,
            fldc_AlertRecipients = @pAlertRecipients,
            fldv_UpdatedBy = @pUpdatedBy,
            fldd_UpdatedDate = @pUpdatedDate
        WHERE fldi_ID = @pID;
        
        IF @@ROWCOUNT = 0
            THROW 50001, 'Safety Performance Indicator not found', 1;
            
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
```

### 3. Delete SPI

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_Delete
(
    @pID NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Delete related data points first
        DELETE FROM tbld_SPIDataPoints WHERE fldi_SPIId = @pID;
        
        -- Delete the SPI
        DELETE FROM tbld_SafetyPerformanceIndicators WHERE fldi_ID = @pID;
        
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50001, 'Safety Performance Indicator not found', 1;
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        THROW;
    END CATCH
END;
```

### 4. Get All SPIs

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldi_ID, fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status,
        fldv_MeasurementUnit, fldv_MeasurementFrequency, fldc_CalculationMethod, fldv_DataSource,
        fldm_TargetValue, fldm_AcceptableRange, fldm_WarningThreshold, fldm_CriticalThreshold,
        fldv_ResponsibleDepartment, fldv_DataOwner, fldv_ReviewAuthority,
        fldd_NextReviewDate, fldd_LastReviewDate, fldc_LastReviewNotes,
        fldb_AlertsEnabled, fldc_AlertRecipients,
        fldv_CreatedBy, fldd_CreatedDate, fldv_UpdatedBy, fldd_UpdatedDate
    FROM tbld_SafetyPerformanceIndicators
    ORDER BY fldv_Name;
END;
```

### 5. Get SPI by ID

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_GetById
(
    @pID NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldi_ID, fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status,
        fldv_MeasurementUnit, fldv_MeasurementFrequency, fldc_CalculationMethod, fldv_DataSource,
        fldm_TargetValue, fldm_AcceptableRange, fldm_WarningThreshold, fldm_CriticalThreshold,
        fldv_ResponsibleDepartment, fldv_DataOwner, fldv_ReviewAuthority,
        fldd_NextReviewDate, fldd_LastReviewDate, fldc_LastReviewNotes,
        fldb_AlertsEnabled, fldc_AlertRecipients,
        fldv_CreatedBy, fldd_CreatedDate, fldv_UpdatedBy, fldd_UpdatedDate
    FROM tbld_SafetyPerformanceIndicators
    WHERE fldi_ID = @pID;
END;
```

### 6. Get SPI by Code

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_GetByCode
(
    @pCode NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldi_ID, fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status,
        fldv_MeasurementUnit, fldv_MeasurementFrequency, fldc_CalculationMethod, fldv_DataSource,
        fldm_TargetValue, fldm_AcceptableRange, fldm_WarningThreshold, fldm_CriticalThreshold,
        fldv_ResponsibleDepartment, fldv_DataOwner, fldv_ReviewAuthority,
        fldd_NextReviewDate, fldd_LastReviewDate, fldc_LastReviewNotes,
        fldb_AlertsEnabled, fldc_AlertRecipients,
        fldv_CreatedBy, fldd_CreatedDate, fldv_UpdatedBy, fldd_UpdatedDate
    FROM tbld_SafetyPerformanceIndicators
    WHERE fldv_Code = @pCode;
END;
```

### 7. Get SPIs by Type

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_GetByType
(
    @pIndicatorType NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldi_ID, fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status,
        fldv_MeasurementUnit, fldv_MeasurementFrequency, fldc_CalculationMethod, fldv_DataSource,
        fldm_TargetValue, fldm_AcceptableRange, fldm_WarningThreshold, fldm_CriticalThreshold,
        fldv_ResponsibleDepartment, fldv_DataOwner, fldv_ReviewAuthority,
        fldd_NextReviewDate, fldd_LastReviewDate, fldc_LastReviewNotes,
        fldb_AlertsEnabled, fldc_AlertRecipients,
        fldv_CreatedBy, fldd_CreatedDate, fldv_UpdatedBy, fldd_UpdatedDate
    FROM tbld_SafetyPerformanceIndicators
    WHERE fldv_IndicatorType = @pIndicatorType
    ORDER BY fldv_Name;
END;
```

### 8. Get SPIs by Department

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_GetByDepartment
(
    @pResponsibleDepartment NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldi_ID, fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status,
        fldv_MeasurementUnit, fldv_MeasurementFrequency, fldc_CalculationMethod, fldv_DataSource,
        fldm_TargetValue, fldm_AcceptableRange, fldm_WarningThreshold, fldm_CriticalThreshold,
        fldv_ResponsibleDepartment, fldv_DataOwner, fldv_ReviewAuthority,
        fldd_NextReviewDate, fldd_LastReviewDate, fldc_LastReviewNotes,
        fldb_AlertsEnabled, fldc_AlertRecipients,
        fldv_CreatedBy, fldd_CreatedDate, fldv_UpdatedBy, fldd_UpdatedDate
    FROM tbld_SafetyPerformanceIndicators
    WHERE fldv_ResponsibleDepartment = @pResponsibleDepartment
    ORDER BY fldv_Name;
END;
```

## SPI Data Points Operations

### 9. Insert SPI Data Point

```sql
CREATE PROCEDURE pr_SPIDataPoint_Insert
(
    @pSPIId INT,
    @pValue DECIMAL(18,4),
    @pMeasurementDate DATETIME2,
    @pPeriod NVARCHAR(50),
    @pDataSource NVARCHAR(255),
    @pEnteredBy NVARCHAR(255),
    @pEnteredDate DATETIME2,
    @pNotes NVARCHAR(MAX) = NULL,
    @pIsVerified BIT = 0,
    @pVerifiedBy NVARCHAR(255) = NULL,
    @pVerifiedDate DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO tbld_SPIDataPoints (
            fldi_SPIId, fldm_Value, fldd_MeasurementDate, fldv_Period, fldv_DataSource,
            fldv_EnteredBy, fldd_EnteredDate, fldc_Notes, fldb_IsVerified, fldv_VerifiedBy, fldd_VerifiedDate
        )
        VALUES (
            @pSPIId, @pValue, @pMeasurementDate, @pPeriod, @pDataSource,
            @pEnteredBy, @pEnteredDate, @pNotes, @pIsVerified, @pVerifiedBy, @pVerifiedDate
        );
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
```

### 10. Get SPI Data Points by SPI ID

```sql
CREATE PROCEDURE pr_SPIDataPoint_GetBySPIId
(
    @pSPIId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldi_ID, fldi_SPIId, fldm_Value, fldd_MeasurementDate, fldv_Period, fldv_DataSource,
        fldv_EnteredBy, fldd_EnteredDate, fldc_Notes, fldb_IsVerified, fldv_VerifiedBy, fldd_VerifiedDate
    FROM tbld_SPIDataPoints
    WHERE fldi_SPIId = @pSPIId
    ORDER BY fldd_MeasurementDate DESC;
END;
```

### 11. Update SPI Data Point

```sql
CREATE PROCEDURE pr_SPIDataPoint_Update
(
    @pID INT,
    @pSPIId INT,
    @pValue DECIMAL(18,4),
    @pMeasurementDate DATETIME2,
    @pPeriod NVARCHAR(50),
    @pDataSource NVARCHAR(255),
    @pEnteredBy NVARCHAR(255),
    @pEnteredDate DATETIME2,
    @pNotes NVARCHAR(MAX) = NULL,
    @pIsVerified BIT = 0,
    @pVerifiedBy NVARCHAR(255) = NULL,
    @pVerifiedDate DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE tbld_SPIDataPoints
        SET 
            fldi_SPIId = @pSPIId,
            fldm_Value = @pValue,
            fldd_MeasurementDate = @pMeasurementDate,
            fldv_Period = @pPeriod,
            fldv_DataSource = @pDataSource,
            fldv_EnteredBy = @pEnteredBy,
            fldd_EnteredDate = @pEnteredDate,
            fldc_Notes = @pNotes,
            fldb_IsVerified = @pIsVerified,
            fldv_VerifiedBy = @pVerifiedBy,
            fldd_VerifiedDate = @pVerifiedDate
        WHERE fldi_ID = @pID;
        
        IF @@ROWCOUNT = 0
            THROW 50001, 'SPI Data Point not found', 1;
            
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
```

### 12. Delete SPI Data Point

```sql
CREATE PROCEDURE pr_SPIDataPoint_Delete
(
    @pID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DELETE FROM tbld_SPIDataPoints WHERE fldi_ID = @pID;
        
        IF @@ROWCOUNT = 0
            THROW 50001, 'SPI Data Point not found', 1;
            
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
```

## Additional Utility Procedures

### 13. Get Active SPIs Requiring Review

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_GetRequiringReview
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldi_ID, fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status,
        fldv_MeasurementUnit, fldv_MeasurementFrequency, fldc_CalculationMethod, fldv_DataSource,
        fldm_TargetValue, fldm_AcceptableRange, fldm_WarningThreshold, fldm_CriticalThreshold,
        fldv_ResponsibleDepartment, fldv_DataOwner, fldv_ReviewAuthority,
        fldd_NextReviewDate, fldd_LastReviewDate, fldc_LastReviewNotes,
        fldb_AlertsEnabled, fldc_AlertRecipients,
        fldv_CreatedBy, fldd_CreatedDate, fldv_UpdatedBy, fldd_UpdatedDate
    FROM tbld_SafetyPerformanceIndicators
    WHERE fldv_Status = 'ACTIVE' 
      AND fldd_NextReviewDate IS NOT NULL 
      AND fldd_NextReviewDate <= GETUTCDATE()
    ORDER BY fldd_NextReviewDate;
END;
```

### 14. Get SPI Dashboard Data

```sql
CREATE PROCEDURE pr_SafetyPerformanceIndicator_GetDashboardData
(
    @pStartDate DATETIME2 = NULL,
    @pEndDate DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @pStartDate IS NULL SET @pStartDate = DATEADD(YEAR, -1, GETUTCDATE());
    IF @pEndDate IS NULL SET @pEndDate = GETUTCDATE();
    
    -- SPI Summary
    SELECT 
        spi.fldi_ID,
        spi.fldv_Code,
        spi.fldv_Name,
        spi.fldv_IndicatorType,
        spi.fldv_Status,
        spi.fldm_TargetValue,
        spi.fldm_WarningThreshold,
        spi.fldm_CriticalThreshold,
        spi.fldv_ResponsibleDepartment,
        
        -- Latest value
        (SELECT TOP 1 fldm_Value 
         FROM tbld_SPIDataPoints dp 
         WHERE dp.fldi_SPIId = spi.fldi_ID 
         ORDER BY dp.fldd_MeasurementDate DESC) AS CurrentValue,
        
        -- Latest measurement date
        (SELECT TOP 1 fldd_MeasurementDate 
         FROM tbld_SPIDataPoints dp 
         WHERE dp.fldi_SPIId = spi.fldi_ID 
         ORDER BY dp.fldd_MeasurementDate DESC) AS LastMeasurementDate,
         
        -- Trend calculation (simplified)
        CASE 
            WHEN (SELECT COUNT(*) FROM tbld_SPIDataPoints dp WHERE dp.fldi_SPIId = spi.fldi_ID AND dp.fldd_MeasurementDate BETWEEN @pStartDate AND @pEndDate) >= 3 THEN
                CASE 
                    WHEN (SELECT TOP 1 fldm_Value FROM tbld_SPIDataPoints dp WHERE dp.fldi_SPIId = spi.fldi_ID ORDER BY dp.fldd_MeasurementDate DESC) >
                         (SELECT fldm_Value FROM (SELECT fldm_Value, ROW_NUMBER() OVER (ORDER BY fldd_MeasurementDate DESC) AS rn 
                                            FROM tbld_SPIDataPoints dp WHERE dp.fldi_SPIId = spi.fldi_ID) t WHERE rn = 3) * 1.05
                    THEN 'IMPROVING'
                    WHEN (SELECT TOP 1 fldm_Value FROM tbld_SPIDataPoints dp WHERE dp.fldi_SPIId = spi.fldi_ID ORDER BY dp.fldd_MeasurementDate DESC) <
                         (SELECT fldm_Value FROM (SELECT fldm_Value, ROW_NUMBER() OVER (ORDER BY fldd_MeasurementDate DESC) AS rn 
                                            FROM tbld_SPIDataPoints dp WHERE dp.fldi_SPIId = spi.fldi_ID) t WHERE rn = 3) * 0.95
                    THEN 'DECLINING'
                    ELSE 'STABLE'
                END
            ELSE 'STABLE'
        END AS TrendDirection
        
    FROM tbld_SafetyPerformanceIndicators spi
    WHERE spi.fldv_Status = 'ACTIVE'
    ORDER BY spi.fldv_Name;
END;
```

## Index Recommendations

```sql
-- Performance indexes
CREATE NONCLUSTERED INDEX IX_SafetyPerformanceIndicators_Code ON tbld_SafetyPerformanceIndicators (fldv_Code);
CREATE NONCLUSTERED INDEX IX_SafetyPerformanceIndicators_Type ON tbld_SafetyPerformanceIndicators (fldv_IndicatorType);
CREATE NONCLUSTERED INDEX IX_SafetyPerformanceIndicators_Department ON tbld_SafetyPerformanceIndicators (fldv_ResponsibleDepartment);
CREATE NONCLUSTERED INDEX IX_SafetyPerformanceIndicators_Status ON tbld_SafetyPerformanceIndicators (fldv_Status);
CREATE NONCLUSTERED INDEX IX_SafetyPerformanceIndicators_NextReview ON tbld_SafetyPerformanceIndicators (fldd_NextReviewDate);

CREATE NONCLUSTERED INDEX IX_SPIDataPoints_SPIId_Date ON tbld_SPIDataPoints (fldi_SPIId, fldd_MeasurementDate DESC);
CREATE NONCLUSTERED INDEX IX_SPIDataPoints_Period ON tbld_SPIDataPoints (fldv_Period);
```

## Sample Data Insert

```sql
-- Insert sample SPI data for testing
INSERT INTO tbld_SafetyPerformanceIndicators (
    fldv_Code, fldv_Name, fldc_Description, fldv_IndicatorType, fldv_Status, fldv_MeasurementUnit, fldv_MeasurementFrequency,
    fldm_TargetValue, fldm_WarningThreshold, fldm_CriticalThreshold, fldv_ResponsibleDepartment, fldv_DataOwner,
    fldv_CreatedBy, fldd_CreatedDate
) VALUES 
('SPI-000001', 'Hazard Report Rate', 'Number of hazard reports submitted per month', 'HAZARD_REPORT_RATE', 'ACTIVE', 'Reports/Month', 'MONTHLY', 10, 5, 2, 'Safety Department', 'SMS Manager', 'SYSTEM', GETUTCDATE()),
('SPI-000002', 'Incident Rate', 'Number of safety incidents per quarter', 'INCIDENT_RATE', 'ACTIVE', 'Incidents/Quarter', 'QUARTERLY', 0, 1, 3, 'Operations', 'Safety Officer', 'SYSTEM', GETUTCDATE()),
('SPI-000003', 'Training Completion Rate', 'Percentage of safety training completed on time', 'TRAINING_COMPLETION_RATE', 'ACTIVE', 'Percentage', 'MONTHLY', 95, 85, 75, 'Human Resources', 'Training Manager', 'SYSTEM', GETUTCDATE());
```

This comprehensive set of stored procedures provides all the functionality needed to support the SafetyPerformanceIndicatorRepository and enables full CRUD operations for the SPI system using the established field naming conventions.