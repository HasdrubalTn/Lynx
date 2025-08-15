[CmdletBinding()]
param()
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot
git config --local core.hooksPath .githooks
Write-Host "✓ Enabled .githooks for this repo"
git config --get core.hooksPath
