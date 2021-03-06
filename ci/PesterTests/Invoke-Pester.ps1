<#
    Invoke Pester Test from VSTS Release Task
#>

# Param([Parameter(Mandatory = $true,
#         ValueFromPipelineByPropertyName = $true,
#         Position = 0)]
#     $TestScript)

$TestScript=".\CI\PesterTests\demo.tests.ps1"

#region Install Pester
Write-Host "Installing Pester from PSGallery"
Install-PackageProvider -Name NuGet -Force -Scope CurrentUser
Install-Module -Name Pester -Scope CurrentUser -Force -SkipPublisherCheck
#endregion

#region call Pester script
Write-Host "Calling Pester test script"
#Invoke-Pester -Script $TestScript -PassThru
$result = Invoke-Pester -Script $TestScript -PassThru -OutputFile ".\ci\PesterTests\Pester-TestResults.xml" -OutputFormat NUnitXML
if ($result.failedCount -ne 0) { 
    Write-Error "Pester returned errors"
}
#endregion