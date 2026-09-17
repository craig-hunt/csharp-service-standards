<#
.SYNOPSIS
    Runs locally what CI runs, in the same order.

.DESCRIPTION
    One command answers whether a change is ready. Running the same steps CI
    runs, in the same order, means a green local run and a red pipeline stop
    disagreeing about what "ready" means.

    Suites run through dotnet run rather than dotnet test. xUnit v3 builds each
    test project as an executable on Microsoft.Testing.Platform, and on the
    .NET 10 SDK dotnet test reports zero tests for these projects while the
    executables themselves discover and run everything. Launching them directly
    is xUnit v3's own documented path and it reports honestly.

.PARAMETER SkipIntegration
    Skips the suites that need Docker.

.PARAMETER SkipMutation
    Skips the mutation run, which is the slowest step by a wide margin.

.EXAMPLE
    .\scripts\verify.ps1
    .\scripts\verify.ps1 -SkipMutation
#>
[CmdletBinding()]
param(
    [switch]$SkipIntegration,
    [switch]$SkipMutation
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot

$configuration = 'Release'

$fastSuites = @(
    'tests/Domain.Tests/Domain.Tests.csproj',
    'tests/Application.Tests/Application.Tests.csproj',
    'tests/Architecture.Tests/Architecture.Tests.csproj',
    'tests/Analyzer.Tests/Analyzer.Tests.csproj',
    'tests/Web.Tests/Web.Tests.csproj'
)

$integrationSuites = @(
    'tests/Integration.Tests/Integration.Tests.csproj'
)

function Invoke-Step {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][scriptblock]$Action
    )

    Write-Host ''
    Write-Host "==> $Name" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED: $Name" -ForegroundColor Red
        Pop-Location
        exit $LASTEXITCODE
    }
}

try {
    # The build carries the lint: warnings are errors, code style is enforced
    # during compilation, and the literal analyzer runs as part of it.
    Invoke-Step 'restore' { dotnet restore }
    Invoke-Step 'build' { dotnet build --configuration $configuration --no-restore }

    foreach ($suite in $fastSuites) {
        Invoke-Step "test $suite" {
            dotnet run --project $suite --configuration $configuration --no-build
        }
    }

    if (-not $SkipIntegration) {
        foreach ($suite in $integrationSuites) {
            Invoke-Step "integration $suite" {
                dotnet run --project $suite --configuration $configuration --no-build
            }
        }
    }
    else {
        Write-Host ''
        Write-Host '==> integration skipped' -ForegroundColor Yellow
    }

    if (-not $SkipMutation) {
        Invoke-Step 'mutation' { dotnet stryker }
    }
    else {
        Write-Host ''
        Write-Host '==> mutation skipped' -ForegroundColor Yellow
    }

    Write-Host ''
    Write-Host 'All checks passed.' -ForegroundColor Green
}
finally {
    Pop-Location
}
