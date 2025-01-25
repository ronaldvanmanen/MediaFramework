$ErrorActionPreference = 'Stop'

Set-Location -LiteralPath $PSScriptRoot

###########################################################################
# CONFIGURATION
###########################################################################

$DotNetDirectory = ".\.dotnet"

$DotNetArchitecture = "<auto>"
$DotNetChannel = "7.0"
$DotNetVersion = "latest"

$DotNetInstallScriptUri = "https://dot.net/v1/dotnet-install.ps1"

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

###########################################################################
# EXECUTION
###########################################################################

# Parse command line parameters
if ($args -join ' ' -match '--architecture ([^ ]+)') {
    $DotNetArchitecture = $Matches[1]
}

# Download dotnet install script
$DotNetInstallScript = "$DotNetDirectory\dotnet-install.ps1"
New-Item -ItemType Directory -Path $DotNetDirectory -Force | Out-Null
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest -Uri $DotNetInstallScriptUri -OutFile $DotNetInstallScript -UseBasicParsing

# Install dotnet
$DotNetInstallDirectory = "$DotNetDirectory\$DotNetArchitecture"
New-Item -ItemType Directory -Path $DotNetInstallDirectory -Force | Out-Null
& powershell $DotNetInstallScript -Channel $DotNetChannel -Version $DotNetVersion -InstallDir $DotNetInstallDirectory -Architecture $DotNetArchitecture
$env:PATH="$DotNetInstallDirectory;$env:PATH"

# Restore dotnet tools (e.g. cake)
& dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

# Run cake
& dotnet cake -- @args
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
