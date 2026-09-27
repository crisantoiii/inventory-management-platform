# =====================================================================
# InventoryPlatform - Provider-Neutral Continuous Verification
# =====================================================================
#
# Sprint 20 S20-T03 - shared verification entry point.
#
# Implements the frozen provider-neutral verification contract:
#
#   1. Restore repository-local .NET tools (src/InventoryPlatform/dotnet-tools.json).
#   2. Restore the solution (src/InventoryPlatform/InventoryPlatform.slnx).
#   3. Build the full solution (normal build; default configuration Release).
#   4. Run all UnitTests (no filter)                      -> TRX.
#   5. Run all Web.Tests (no filter)                      -> TRX.
#   6. Run IntegrationTests with affirmative filter
#      "TestTier=ProviderNeutral" (positive inclusion;
#      the classification audit runs as part of this suite) -> TRX.#  7. Run EF "migrations has-pending-model-changes"
#      (Infrastructure project + Web startup, --no-build,
#      sequentially AFTER build/tests complete; executed from the
#      tool-manifest directory so the repository-local dotnet-ef
#      version is used, never a globally installed one).
#   8. Preserve distinct TRX results per test project.
#   9. Fail with a non-zero exit code if any mandatory step fails.
#  10. Emit a final verification-tier summary.
#
# Explicitly NOT part of this gate:
#   - SQL Server relational tests ("TestTier=SqlServerRelational" is
#     never executed here); no SQL relational-pass claim is made.
#   - Browser/E2E tests.
#   - Any LocalDB / SQL Server / external service dependency.
#
# Invocation is independent of the caller's working directory: all
# repository paths are resolved relative to this script's location.
#
# Exit codes:
#   0 = provider-neutral verification completed successfully
#   1 = a mandatory verification step failed
#   2 = the script could not resolve a required repository file
#
# =====================================================================

[CmdletBinding()]
param(
    # Build configuration for restore, build, tests, and EF verification.
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    # Repository-relative (or absolute) directory that receives the TRX
    # result files. Defaults to "artifacts/verification" under the
    # repository root, suitable for direct GitHub Actions artifact upload.
    [string]$ResultsDirectory = 'artifacts/verification',

    # Controlled local-reuse switch: skips ONLY the two restore steps
    # (dotnet tool restore / dotnet restore). It must not, and does not,
    # skip build, tests, or EF verification. Intended for controlled local
    # iteration where dependencies/tools are already restored; CI (T04)
    # should use normal restore behavior.
    [switch]$SkipRestore
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------
# Repository path resolution (independent of caller working directory)
# ---------------------------------------------------------------------

$scriptPath = $PSCommandPath
if (-not $scriptPath) {
    # Windows PowerShell 5.1 fallback when $PSCommandPath is unavailable.
    $scriptPath = $MyInvocation.MyCommand.Path
}

if (-not $scriptPath) {
    Write-Error 'Unable to determine the verification script location. Invoke the script by path.'
    exit 2
}

$repositoryRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
if ([string]::IsNullOrWhiteSpace($repositoryRoot) -or -not (Test-Path -LiteralPath $repositoryRoot)) {
    Write-Error "Unable to resolve the repository root from script location '$scriptPath'."
    exit 2
}

# Required repository authorities. Fail clearly if any cannot be located.
$solutionPath = Join-Path $repositoryRoot 'src/InventoryPlatform/InventoryPlatform.slnx'
$dotnetToolsManifestPath = Join-Path $repositoryRoot 'src/InventoryPlatform/dotnet-tools.json'
$migrationsProjectPath = Join-Path $repositoryRoot 'src/InventoryPlatform/InventoryPlatform.Infrastructure/InventoryPlatform.Infrastructure.csproj'
$dotnetToolsManifestDirectory = Split-Path -Parent $dotnetToolsManifestPath
$startupProjectPath = Join-Path $repositoryRoot 'src/InventoryPlatform/InventoryPlatform.Web/InventoryPlatform.Web.csproj'
$unitTestsProjectPath = Join-Path $repositoryRoot 'tests/InventoryPlatform.UnitTests/InventoryPlatform.UnitTests.csproj'
$webTestsProjectPath = Join-Path $repositoryRoot 'tests/InventoryPlatform.Web.Tests/InventoryPlatform.Web.Tests.csproj'
$integrationTestsProjectPath = Join-Path $repositoryRoot 'tests/InventoryPlatform.IntegrationTests/InventoryPlatform.IntegrationTests.csproj'

$requiredPaths = @(
    @{ Path = $solutionPath;              Description = 'solution' },
    @{ Path = $dotnetToolsManifestPath;   Description = '.NET local tools manifest' },
    @{ Path = $migrationsProjectPath;     Description = 'EF migrations project' },
    @{ Path = $startupProjectPath;        Description = 'EF startup project' },
    @{ Path = $unitTestsProjectPath;      Description = 'UnitTests project' },
    @{ Path = $webTestsProjectPath;       Description = 'Web.Tests project' },
    @{ Path = $integrationTestsProjectPath; Description = 'IntegrationTests project' }
)

foreach ($required in $requiredPaths) {
    if (-not (Test-Path -LiteralPath $required.Path -PathType Leaf)) {
        Write-Error ("Provider-neutral verification cannot start: required {0} was not found at '{1}'." -f $required.Description, $required.Path)
        exit 2
    }
}

# Results directory: resolve against the repository root when relative.
if ([System.IO.Path]::IsPathRooted($ResultsDirectory)) {
    $resolvedResultsDirectory = $ResultsDirectory
} else {
    $resolvedResultsDirectory = Join-Path $repositoryRoot $ResultsDirectory
}

try {
    New-Item -ItemType Directory -Path $resolvedResultsDirectory -Force | Out-Null
} catch {
    Write-Error ("Provider-neutral verification cannot start: results directory '{0}' could not be created. {1}" -f $resolvedResultsDirectory, $_.Exception.Message)
    exit 1
}

# Distinct, deterministic TRX filenames (no collisions; predictable
# handling of existing results - a rerun overwrites its own files).
$unitTestsTrxPath = Join-Path $resolvedResultsDirectory 'unit-tests.trx'
$webTestsTrxPath = Join-Path $resolvedResultsDirectory 'web-tests.trx'
$providerNeutralIntegrationTrxPath = Join-Path $resolvedResultsDirectory 'integration-tests-provider-neutral.trx'

Write-Host '====================================================================='
Write-Host ' InventoryPlatform - Provider-Neutral Continuous Verification'
Write-Host '====================================================================='
Write-Host (" Repository root : {0}" -f $repositoryRoot)
Write-Host (" Configuration   : {0}" -f $Configuration)
Write-Host (" Results dir     : {0}" -f $resolvedResultsDirectory)
Write-Host (" SkipRestore     : {0}" -f $SkipRestore.IsPresent)
Write-Host '====================================================================='

# ---------------------------------------------------------------------
# Failure semantics
# ---------------------------------------------------------------------
# Native dotnet failures must reliably stop the script with a non-zero
# exit code. $ErrorActionPreference = 'Stop' does NOT stop on non-zero
# native exit codes, so every mandatory command is invoked through this
# helper, which inspects $LASTEXITCODE explicitly, preserves all output,
# and stops further verification when continuing would be misleading.

$script:failedCommandDescription = $null

function Invoke-VerifiedCommand {
    param(
        [string]$Description,
        [string[]]$ArgumentList,
        [string]$WorkingDirectory
    )

    Write-Host ''
    Write-Host (" >>> {0}" -f $Description)
    Write-Host ("     {0} {1}" -f 'dotnet', ($ArgumentList -join ' '))
    if ($WorkingDirectory) {
        Write-Host ("     (working directory: {0})" -f $WorkingDirectory)
    }

    # Stream the command with its own output preserved (no re-wrapping
    # that could swallow exit codes or hide test output). When a working
    # directory is required (EF must run under the tool-manifest directory
    # so the repository-local dotnet-ef resolves), set it only for the
    # command and always restore the caller's location.
    $previousLocation = $null
    try {
        if ($WorkingDirectory) {
            $previousLocation = (Get-Location).Path
            Set-Location -LiteralPath $WorkingDirectory
        }

        & dotnet @ArgumentList
        $nativeExitCode = $LASTEXITCODE
    } finally {
        if ($previousLocation) {
            Set-Location -LiteralPath $previousLocation
        }
    }

    if ($nativeExitCode -ne 0) {
        $script:failedCommandDescription = $Description
        Write-Host ''
        Write-Host (" XXX MANDATORY STEP FAILED (exit code {0}): {1}" -f $nativeExitCode, $Description)
        exit 1
    }

    Write-Host (" <<< OK: {0}" -f $Description)
}

# ---------------------------------------------------------------------
# Frozen verification sequence
# ---------------------------------------------------------------------

# 1. Restore repository-local .NET tools (controlled skip allowed).
if ($SkipRestore) {
    Write-Host ''
    Write-Host ' >>> SKIPPED BY -SkipRestore: dotnet tool restore (controlled local reuse; build/tests/EF remain mandatory).'
} else {
    Invoke-VerifiedCommand -Description 'Restore repository-local .NET tools (src/InventoryPlatform/dotnet-tools.json)' `
        -ArgumentList @('tool', 'restore', '--tool-manifest', $dotnetToolsManifestPath)
}

# 1a. Prove the repository-local dotnet-ef tool resolves from the tool
#     manifest directory (accepted version: dotnet-ef 10.0.10; the global
#     tool must never be used). Always mandatory: if tools are not restored
#     (including under -SkipRestore), this fails with a clear message.
Invoke-VerifiedCommand -Description 'Verify repository-local dotnet-ef tool resolves (dotnet ef --version)' `
    -ArgumentList @('ef', '--version') `
    -WorkingDirectory $dotnetToolsManifestDirectory

# 2. Restore solution dependencies (controlled skip allowed).
if ($SkipRestore) {
    Write-Host ''
    Write-Host ' >>> SKIPPED BY -SkipRestore: dotnet restore (controlled local reuse; build/tests/EF remain mandatory).'
} else {
    Invoke-VerifiedCommand -Description 'Restore solution dependencies (src/InventoryPlatform/InventoryPlatform.slnx)' `
        -ArgumentList @('restore', $solutionPath)
}

# 3. Normal build of the full solution (non-incremental build is NOT
#    mandatory; no warning suppression; historical warnings are not a
#    remediation target of this script).
Invoke-VerifiedCommand -Description ('Build full solution ({0})' -f $Configuration) `
    -ArgumentList @('build', $solutionPath, '--configuration', $Configuration, '--no-restore')

# 4. UnitTests - all tests, no filter.
Invoke-VerifiedCommand -Description 'Run UnitTests (all tests, no filter)' `
    -ArgumentList @('test', $unitTestsProjectPath,
        '--configuration', $Configuration,
        '--no-build',
        '--results-directory', $resolvedResultsDirectory,
        '--logger', ('trx;LogFileName={0}' -f (Split-Path -Leaf $unitTestsTrxPath)))

# 5. Web.Tests - all tests, no filter, no LocalDB/SQL dependencies.
Invoke-VerifiedCommand -Description 'Run Web.Tests (all tests, no filter)' `
    -ArgumentList @('test', $webTestsProjectPath,
        '--configuration', $Configuration,
        '--no-build',
        '--results-directory', $resolvedResultsDirectory,
        '--logger', ('trx;LogFileName={0}' -f (Split-Path -Leaf $webTestsTrxPath)))

# 6. Provider-Neutral IntegrationTests - AFFIRMATIVE inclusion only.
#    The gate must never be implemented by excluding SqlServerRelational;
#    the classification audit runs inside this suite.
Invoke-VerifiedCommand -Description 'Run IntegrationTests with affirmative filter TestTier=ProviderNeutral' `
    -ArgumentList @('test', $integrationTestsProjectPath,
        '--configuration', $Configuration,
        '--no-build',
        '--filter', 'TestTier=ProviderNeutral',
        '--results-directory', $resolvedResultsDirectory,
        '--logger', ('trx;LogFileName={0}' -f (Split-Path -Leaf $providerNeutralIntegrationTrxPath)))

# 7. EF pending-model verification - sequentially, AFTER all build/test
#    processes have completed. Uses the frozen Infrastructure (migrations)
#    and Web (startup) projects, the selected configuration, and --no-build
#    against the matching solution build. No database connection is
#    required by has-pending-model-changes. The command runs from the
#    tool-manifest directory so the repository-local dotnet-ef resolves.
Invoke-VerifiedCommand -Description 'EF: migrations has-pending-model-changes (Infrastructure / Web startup, --no-build)' `
    -ArgumentList @('ef', 'migrations', 'has-pending-model-changes',
        '--project', $migrationsProjectPath,
        '--startup-project', $startupProjectPath,
        '--configuration', $Configuration,
        '--no-build') `
    -WorkingDirectory $dotnetToolsManifestDirectory

# ---------------------------------------------------------------------
# Final verification-tier summary
# ---------------------------------------------------------------------

Write-Host ''
Write-Host '====================================================================='
Write-Host ' Provider-neutral verification completed successfully.'
Write-Host '====================================================================='
Write-Host ' Executed tiers / steps:'
Write-Host '   [OK] Solution restore'
if (-not $SkipRestore) {
    Write-Host '   [OK] Repository-local .NET tool restore (dotnet-ef 10.0.10) + tool resolution evidence'
} else {
    Write-Host '   [SKIPPED by -SkipRestore] Repository-local .NET tool restore'
}
Write-Host '   [OK] Normal full-solution build'
Write-Host '   [OK] UnitTests (all tests, no filter)'
Write-Host '   [OK] Web.Tests (all tests, no filter)'
Write-Host '   [OK] IntegrationTests - affirmative filter TestTier=ProviderNeutral (classification audit included)'
Write-Host '   [OK] EF migrations has-pending-model-changes (Infrastructure / Web startup, --no-build)'
Write-Host ''
Write-Host ' NOT EXECUTED / not part of this gate:'
Write-Host '   [--] SQL Server relational tests (TestTier=SqlServerRelational) were NOT EXECUTED.'
Write-Host '   [--] Browser/E2E tests were NOT EXECUTED (not part of this gate).'
Write-Host ''
Write-Host ' No SQL Server relational-pass claim is being made.'
Write-Host (' Provider-neutral verification completed successfully. SQL Server relational tests were not executed.')
Write-Host '====================================================================='

exit 0
