# build-local.ps1
<#
.SYNOPSIS
Builds and packs the PocketBase.Blazor project.
.DESCRIPTION
Cleans, builds, and creates a NuGet package in ./nupkg.
#>

# Stop on errors
$ErrorActionPreference = "Stop"

# Variables
$ProjectPath = "./src/PocketBase.Blazor/PocketBase.Blazor.csproj"
$OutputDir = "./nupkg"

Write-Host "Cleaning project..." -ForegroundColor Cyan
dotnet clean $ProjectPath

Write-Host "Building project in Release configuration..." -ForegroundColor Cyan
dotnet build $ProjectPath -c Release

Write-Host "Packing project..." -ForegroundColor Cyan
dotnet pack $ProjectPath -c Release -o $OutputDir

Write-Host "✅ Package ready in $OutputDir" -ForegroundColor Green
