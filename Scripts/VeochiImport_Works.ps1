#cd C:\Projects\PDXSMS_V2\Scripts 
#.\Import-HazardReportsToAPI.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\HazardReportSubmissionForm.csv" -ApiBaseUrl "http://localhost:5115" -ApiKey "SMS-DEV-12345-ABCDEF" -BatchSize 2

cd C:\Projects\PDXSMS_V2\Scripts 
.\Import-HazardReportsToAPI_WITH_DEFAULTS.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\HazardReportSubmissionForm.csv" -ApiBaseUrl "http://localhost:5115" -ApiKey "SMS-DEV-12345-ABCDEF" -BatchSize 2