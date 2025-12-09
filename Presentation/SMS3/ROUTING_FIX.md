**Now the URL patterns should work correctly:**

1. **`/SMSRiskManagement/PreliminaryAssessment`** ? Default page
2. **`/SMSRiskManagement/PreliminaryAssessment/RS-001`** ? Load specific assessment  
3. **`/SMSRiskManagement/PreliminaryAssessment/RS-001/HZ-0547`** ? Load assessment with hazard context

**Your problematic URL:** `/SMSRiskManagement/PreliminaryAssessment/New/1?hazardId=HZ-0547&reportId=RP-0269`

**Should become:** `/SMSRiskManagement/PreliminaryAssessment/RS-001?hazardId=HZ-0547` 

Where `RS-001` is the actual **Initial** RiskAssessment ID created for hazard `HZ-0547`.

**To fix your current URL, you need to:**

1. **Find the Initial RiskAssessment ID** for hazard `HZ-0547`
2. **Navigate to** `/SMSRiskManagement/PreliminaryAssessment/{AssessmentId}`

Let me create a simple page to help you find the correct assessment ID. Would you like me to create a lookup utility or should we go directly to the database to find the actual RiskAssessment ID for `HZ-0547`?

The current routing I've fixed should handle all these patterns properly now. **Try navigating to:**

`https://localhost:7178/SMSRiskManagement/PreliminaryAssessment/RS-001`

(Replace `RS-001` with the actual Initial RiskAssessment ID for your hazard)