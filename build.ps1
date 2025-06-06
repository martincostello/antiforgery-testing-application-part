#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][string] $OutputPath = "",
    [Parameter(Mandatory = $false)][switch] $SkipTests
)

$Configuration = "Release"
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$solutionFile = Join-Path $solutionPath "TodoApp.slnx"
$sdkFile = Join-Path $solutionPath "global.json"
$testProject = Join-Path $solutionPath "tests" "TodoApp.Tests" "TodoApp.Tests.csproj"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

if ($OutputPath -eq "") {
    $OutputPath = Join-Path $solutionPath "artifacts"
}

$installDotNetSdk = $false;

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Output "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Output "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk) {

    ${env:DOTNET_INSTALL_DIR} = Join-Path $solutionPath ".dotnet"
    $sdkPath = Join-Path ${env:DOTNET_INSTALL_DIR} "sdk" $dotnetVersion

    if (-Not (Test-Path $sdkPath)) {
        if (-Not (Test-Path ${env:DOTNET_INSTALL_DIR})) {
            mkdir ${env:DOTNET_INSTALL_DIR} | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        if (($PSVersionTable.PSVersion.Major -ge 6) -And (-Not $IsWindows)) {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --jsonfile $sdkFile --install-dir ${env:DOTNET_INSTALL_DIR} --no-path --skip-non-versioned-files
        }
        else {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -JsonFile $sdkFile -InstallDir ${env:DOTNET_INSTALL_DIR} -NoPath -SkipNonVersionedFiles
        }
    }
}
else {
    ${env:DOTNET_INSTALL_DIR} = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path ${env:DOTNET_INSTALL_DIR} "dotnet"

if ($installDotNetSdk) {
    ${env:PATH} = "${env:DOTNET_INSTALL_DIR};${env:PATH}"
}

function DotNetBuild {
    param([string]$Project)

    & $dotnet build $Project

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

function DotNetTest {
    param([string]$Project)

    $additionalArgs = @()

    if (-Not [string]::IsNullOrEmpty($env:GITHUB_SHA)) {
        $additionalArgs += "--logger:GitHubActions;report-warnings=false"
    }

    & $dotnet test $Project --output $OutputPath --configuration $Configuration $additionalArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

Write-Output "Building solution..."
DotNetBuild $solutionFile

if (-Not $SkipTests) {
    Write-Output "Running tests..."
    DotNetTest $testProject
}
