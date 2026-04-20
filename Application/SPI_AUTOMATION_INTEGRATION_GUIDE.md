# Safety Performance Indicator (SPI) Automation System

## ?? Overview

The SPI Automation System provides automated calculation and data point creation for Safety Performance Indicators based on SMS events and scheduled calculations. This system automatically updates SPIs when hazards, risk assessments, and mitigations are processed in the SMS.

## ??? Architecture

### Components Created:

1. **Core Services**
   - `SPIAutomationService` - Central automation logic
   - `SPIEventCoordinator` - Event orchestration and integration interface

2. **Event Handlers**
   - `HazardEventSPIHandler` - Handles hazard creation/closure events
   - `RiskAssessmentEventSPIHandler` - Handles assessment completion events
   - `MitigationEventSPIHandler` - Handles mitigation completion/overdue events

3. **Background Services**
   - `SPICalculationBackgroundService` - Scheduled calculations and maintenance

4. **Event Models**
   - `HazardCreatedEvent`, `HazardClosedEvent`
   - `RiskAssessmentCompletedEvent`, `HighRiskIdentifiedEvent`
   - `MitigationCompletedEvent`, `MitigationOverdueEvent`, `MitigationStatusChangedEvent`

## ?? Integration Points

### 1. Hazard Reporting Integration

**File: `HazardReporting.razor.cs`**

Add this call after successful hazard creation:

```csharp
// In your hazard creation success handler
private readonly SPIEventCoordinator _spiCoordinator;

// After successful hazard creation
await _spiCoordinator.OnHazardCreated(
    hazardId: newHazard.Code,
    hazardCode: newHazard.Code, 
    createdDate: newHazard.CreatedDate,
    createdBy: newHazard.CreatedBy,
    reportId: reportId,
    hazardType: newHazard.HazardType,
    hazardCategory: newHazard.HazardCategory
);
```

### 2. Risk Assessment Integration

**File: `TechnicalAssessment.razor.cs` or similar**

Add this call after risk assessment completion:

```csharp
// After successful risk assessment completion
await _spiCoordinator.OnRiskAssessmentCompleted(
    assessmentId: assessment.Code,
    assessmentCode: assessment.Code,
    startDate: assessment.StartDate,
    completedDate: DateTime.UtcNow,
    targetCompletionDate: assessment.TargetDate,
    completedBy: CurrentUser.UserCode,
    riskLevel: assessment.FinalRiskLevel,
    riskScore: assessment.RiskScore
);

// If high risk is identified
if (IsHighRisk(assessment.FinalRiskLevel))
{
    await _spiCoordinator.OnHighRiskIdentified(
        assessmentId: assessment.Code,
        hazardId: assessment.HazardId,
        riskLevel: assessment.FinalRiskLevel,
        riskScore: assessment.RiskScore,
        identifiedDate: DateTime.UtcNow,
        identifiedBy: CurrentUser.UserCode
    );
}
```

### 3. Mitigation Integration

**File: Mitigation update/completion handlers**

Add these calls for mitigation events:

```csharp
// When mitigation is completed
await _spiCoordinator.OnMitigationCompleted(
    mitigationId: mitigation.Code,
    mitigationCode: mitigation.Code,
    hazardId: mitigation.HazardCode,
    targetCompletionDate: mitigation.TargetCompletionDate,
    completedDate: DateTime.UtcNow,
    completedBy: CurrentUser.UserCode
);

// When mitigation status changes
await _spiCoordinator.OnMitigationStatusChanged(
    mitigationId: mitigation.Code,
    mitigationCode: mitigation.Code,
    hazardId: mitigation.HazardCode,
    oldStatus: oldStatus,
    newStatus: mitigation.Status,
    statusChangedDate: DateTime.UtcNow,
    changedBy: CurrentUser.UserCode,
    targetCompletionDate: mitigation.TargetCompletionDate
);
```

## ?? Quick Integration Methods

For rapid integration, use these simple notification methods:

```csharp
// Simple hazard creation notification
await _spiCoordinator.NotifyHazardCreated(hazardCode, createdBy, reportId);

// Simple risk assessment completion notification
await _spiCoordinator.NotifyRiskAssessmentCompleted(assessmentCode, completedBy, riskLevel);

// Simple mitigation completion notification  
await _spiCoordinator.NotifyMitigationCompleted(mitigationCode, completedBy, targetDate);
```

## ?? Dependency Injection Setup

**? ALREADY CONFIGURED!** 

The SPI Automation services have been automatically registered in the Application layer DI configuration:

**File: `Application/Configuration/DependencyInjection.cs`**

```csharp
// SPI Automation Services - Event-driven safety performance indicators
services.AddScoped<ISPIAutomationService, SPIAutomationService>();
services.AddScoped<SPIEventCoordinator>();

// SPI Event Handlers - Automated SPI calculations from SMS events
services.AddScoped<HazardEventSPIHandler>();
services.AddScoped<RiskAssessmentEventSPIHandler>();
services.AddScoped<MitigationEventSPIHandler>();

// SPI Background Services - Scheduled automation calculations
services.AddHostedService<SPICalculationBackgroundService>();
```

**No changes needed to `Program.cs`** - The services are automatically available through the existing `AddApplicationServices()` call.

## ?? Automated SPIs

### Hazard-Related SPIs:
- **Hazard Report Rate** - Daily count of submitted hazards
- **Hazard Closure Time** - Average days to close hazards

### Risk Assessment SPIs:
- **Risk Assessment Completion Rate** - Percentage completed on time
- **High Risk Exposure** - Count of Critical/High risk identifications

### Mitigation SPIs:
- **Mitigation Implementation Rate** - Effectiveness score based on timeliness
- **Corrective Action Closure** - Percentage of actions closed on time

## ? Background Calculations

The system runs automated calculations:

- **Daily (6 AM)**: Hazard report rates, closure rates
- **Weekly (Monday 7 AM)**: Weekly trend analysis
- **Monthly (1st at 8 AM)**: Monthly performance summaries
- **Daily (9 AM)**: Overdue mitigation checks

## ?? Implementation Steps

1. **? DI Registration** - Already configured in Application layer
2. **Integrate Event Calls** - Add coordinator calls to existing operations
3. **Test Integration** - Verify SPIs update when events occur
4. **Monitor Logs** - Check for automation success/failures
5. **Customize Calculations** - Adjust scoring algorithms as needed

## ?? Logging

All automation operations are extensively logged:
- ?? Event processing start/completion
- ? Successful SPI updates
- ? Error handling and failures
- ?? Background calculation results

Log levels: Information, Warning, Error with structured data.

## ?? TODOs for Complete Implementation

1. **Implement Data Queries** - Replace placeholder methods in `SPIAutomationService`
2. **Add CQRS Queries** - Create queries for counting hazards, assessments, mitigations
3. **Connect to Event System** - If you have an existing event system, integrate with it
4. **Create Custom SPIs** - Add any additional SPI types needed
5. **Dashboard Integration** - Connect automated SPIs to your dashboard/reporting

## ?? Benefits

- ? **Real-time Updates** - SPIs update immediately when events occur
- ? **Consistent Data** - Standardized calculation methods
- ? **Reduced Manual Work** - Automatic data point creation
- ? **Historical Tracking** - Complete audit trail of SPI changes
- ? **Trend Analysis** - Automated calculation of performance trends
- ? **Regulatory Compliance** - Consistent metrics for compliance reporting

## ?? Next Steps

1. Start with one SPI type (e.g., Hazard Report Rate)
2. Integrate the event calls in your hazard creation process
3. Verify the automation works correctly
4. Gradually add other SPI types
5. Monitor and tune the scoring algorithms
6. Add dashboard visualization of automated SPIs