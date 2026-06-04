
#cd C:\Projects\PDXSMS_V2\Scripts 
#.\Import-HazardReportsToAPI_V2.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\VeociExtract_051226.csv" -ApiBaseUrl "https://sms-test.portofportland.com" -ApiKey "SMS-DEV-12345-ABCDEF" -BatchSize 2

cd C:\Projects\PDXSMS_V2\Scripts 
.\Import-HazardReportsToAPI_V2.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\VeociExtract_06.csv" -ApiBaseUrl "https://localhost:7178" -ApiKey "sms-tODrESNx9XBCfSs5FaC9Y" -BatchSize 2

#cd C:\Projects\PDXSMS_V2\Scripts 
#.\Import-HazardReportsToAPI_V2.ps1 -CsvFilePath "C:\Projects\PDXSMS_V2\Shared\VeociExtract_051226.csv" -ApiBaseUrl "https://sms.portofportland.com" -ApiKey "SMS-DEV-12345-ABCDEF" -BatchSize 2