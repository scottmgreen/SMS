#cd C:\Projects\PDXSMS_V2\Scripts 
#.\Import-HazardReportsToAPI.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\HazardReportSubmissionForm.csv" -ApiBaseUrl "http://localhost:5115" -ApiKey "SMS-DEV-12345-ABCDEF" -BatchSize 2

#cd C:\Projects\PDXSMS_V2\Scripts 
#.\Import-HazardReportsToAPI_V2.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\VeociExtract_051226.csv" -ApiBaseUrl "http://10.122.54.111" -ApiKey "sms-tODrESNx9XBCfSs5FaC9Y" -BatchSize 2

cd C:\Projects\PDXSMS_V2\Scripts 
.\Import-HazardReportsToAPI_V2.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\VeociExtract_06.csv" -ApiBaseUrl "http://localhost:5115" -ApiKey "sms-tODrESNx9XBCfSs5FaC9Y" -BatchSize 2