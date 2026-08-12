# Run in elevated PowerShell (Run as Administrator)

$smtpUser = Read-Host "Enter SMTP username"
$smtpPass = Read-Host "Enter SMTP password"

# Standard keys used by app configuration binding
[Environment]::SetEnvironmentVariable("SmtpEmailConfiguration__Username", $smtpUser, "Machine")
[Environment]::SetEnvironmentVariable("SmtpEmailConfiguration__Password", $smtpPass, "Machine")

Write-Host "Machine-level SMTP variables saved."

# Verify values exist (password masked)
$userCheck = [Environment]::GetEnvironmentVariable("SmtpEmailConfiguration__Username", "Machine")
$passCheck = [Environment]::GetEnvironmentVariable("SmtpEmailConfiguration__Password", "Machine")

Write-Host ("Username: " + ($(if([string]::IsNullOrWhiteSpace($userCheck)){"***MISSING***"}else{$userCheck})))
Write-Host ("Password: " + ($(if([string]::IsNullOrWhiteSpace($passCheck)){"***MISSING***"}else{"***SET***"})))

# Recycle IIS app pool so new env vars are picked up (edit app pool name)
Import-Module WebAdministration
Restart-WebAppPool -Name "YourAppPoolName"