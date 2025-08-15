param(
  [Parameter(Mandatory=$true)] [string] $IssueNumber,
  [Parameter(Mandatory=$true)] [string] $Slug
)
$branch = "feature/$IssueNumber-$Slug"
git fetch origin
git checkout -b $branch origin/develop
Write-Host "Created and switched to $branch"
