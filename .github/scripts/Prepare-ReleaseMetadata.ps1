[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$ReleaseJsonPath,
  [Parameter(Mandatory = $true)][string]$CsprojPath,
  [Parameter(Mandatory = $true)][string]$Repository,
  [string]$NotesPt = "",
  [string]$NotesEn = "",
  [string]$PublishedAt = (Get-Date -Format "yyyy-MM-dd")
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ReleaseJsonPath)) { throw "release.json not found: $ReleaseJsonPath" }
if (-not (Test-Path $CsprojPath)) { throw "csproj not found: $CsprojPath" }

$jsonText = Get-Content -Path $ReleaseJsonPath -Raw -Encoding utf8
$release = $jsonText | ConvertFrom-Json

if ($release.version -notmatch '^(\d+)\.(\d+)\.(\d+)$') {
  throw "version must be MAJOR.MINOR.PATCH, got: '$($release.version)'"
}

$major = [int]$Matches[1]
$minor = [int]$Matches[2]
$patch = [int]$Matches[3] + 1
$version = "$major.$minor.$patch"
$tag = "v$version"
$fileName = "PhasmoHunt-v$version-win-x64.zip"
$downloadUrl = "https://github.com/$Repository/releases/download/$tag/$fileName"

if (-not $NotesPt) { $NotesPt = "Release automatica $tag" }
if (-not $NotesEn) { $NotesEn = "Automatic release $tag" }

$notesObj = [pscustomobject]@{ pt = $NotesPt; en = $NotesEn }
$historyEntry = [pscustomobject]@{
  version     = $version
  tag         = $tag
  publishedAt = $PublishedAt
  notes       = $notesObj
}

$history = @($historyEntry) + @($release.history)
if ($null -eq $release.history) { $history = @($historyEntry) }

$release.version = $version
$release.tag = $tag
$release.publishedAt = $PublishedAt
$release.fileName = $fileName
$release.downloadUrl = $downloadUrl
$release.notes = $notesObj
$release.history = $history

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText((Resolve-Path $ReleaseJsonPath), ($release | ConvertTo-Json -Depth 20), $utf8NoBom)

$csproj = Get-Content -Path $CsprojPath -Raw -Encoding utf8
$csproj = [regex]::Replace($csproj, '<Version>[^<]*</Version>', "<Version>$version</Version>")
$csproj = [regex]::Replace($csproj, '<AssemblyVersion>[^<]*</AssemblyVersion>', "<AssemblyVersion>$version.0</AssemblyVersion>")
$csproj = [regex]::Replace($csproj, '<FileVersion>[^<]*</FileVersion>', "<FileVersion>$version.0</FileVersion>")
[System.IO.File]::WriteAllText((Resolve-Path $CsprojPath), $csproj, $utf8NoBom)

if ($env:GITHUB_OUTPUT) {
  "version=$version" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
  "tag=$tag" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
  "file_name=$fileName" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
  "download_url=$downloadUrl" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
}

[pscustomobject]@{
  Version     = $version
  Tag         = $tag
  FileName    = $fileName
  DownloadUrl = $downloadUrl
}
