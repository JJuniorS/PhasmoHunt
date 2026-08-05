$ErrorActionPreference = "Stop"

Describe "Prepare-ReleaseMetadata" {
  BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot "..\Prepare-ReleaseMetadata.ps1"
  }

  BeforeEach {
    $tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("phasmo-release-" + [guid]::NewGuid())
    New-Item -ItemType Directory -Path $tmp | Out-Null
    Copy-Item (Join-Path $PSScriptRoot "fixtures\release.sample.json") (Join-Path $tmp "release.json")
    Copy-Item (Join-Path $PSScriptRoot "fixtures\PhasmoHunt.sample.csproj") (Join-Path $tmp "PhasmoHunt.csproj")
    $script:tmp = $tmp
  }

  AfterEach {
    Remove-Item -Recurse -Force $script:tmp
  }

  It "bumps patch and rewrites release.json + csproj" {
    $result = & $scriptPath `
      -ReleaseJsonPath (Join-Path $script:tmp "release.json") `
      -CsprojPath (Join-Path $script:tmp "PhasmoHunt.csproj") `
      -Repository "JJuniorS/PhasmoHunt" `
      -NotesPt "Release automatica v1.1.2" `
      -NotesEn "Automatic release v1.1.2" `
      -PublishedAt "2026-08-05"

    $result.Version | Should -Be "1.1.2"
    $result.Tag | Should -Be "v1.1.2"
    $result.FileName | Should -Be "PhasmoHunt-v1.1.2-win-x64.zip"
    $result.DownloadUrl | Should -Be "https://github.com/JJuniorS/PhasmoHunt/releases/download/v1.1.2/PhasmoHunt-v1.1.2-win-x64.zip"

    $json = Get-Content (Join-Path $script:tmp "release.json") -Raw | ConvertFrom-Json
    $json.version | Should -Be "1.1.2"
    $json.tag | Should -Be "v1.1.2"
    $json.fileName | Should -Be "PhasmoHunt-v1.1.2-win-x64.zip"
    $json.downloadUrl | Should -Be $result.DownloadUrl
    $json.publishedAt | Should -Be "2026-08-05"
    $json.notes.pt | Should -Be "Release automatica v1.1.2"
    $json.history.Count | Should -Be 2
    $json.history[0].version | Should -Be "1.1.2"

    $csproj = Get-Content (Join-Path $script:tmp "PhasmoHunt.csproj") -Raw
    $csproj | Should -Match "<Version>1\.1\.2</Version>"
    $csproj | Should -Match "<AssemblyVersion>1\.1\.2\.0</AssemblyVersion>"
    $csproj | Should -Match "<FileVersion>1\.1\.2\.0</FileVersion>"
  }

  It "fails on non-semver version" {
    $path = Join-Path $script:tmp "release.json"
    $bad = Get-Content $path -Raw | ConvertFrom-Json
    $bad.version = "nope"
    $bad | ConvertTo-Json -Depth 10 | Set-Content $path -Encoding utf8
    { & $scriptPath -ReleaseJsonPath $path -CsprojPath (Join-Path $script:tmp "PhasmoHunt.csproj") -Repository "o/r" } | Should -Throw
  }
}
