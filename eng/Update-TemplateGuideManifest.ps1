param(
    [string]$DocsRoot = "docs/guides/template",
    [string]$DocsReleaseFile = ".docs.release",
    [string]$TemplateReleaseFile = ".template.release",
    [string]$ManifestPath = "docs/guides/template/guide-manifest.json"
)

$ErrorActionPreference = "Stop"

function Read-SingleLine {
    param([string]$Path, [string]$Name)

    if (-not (Test-Path -LiteralPath $Path)) { throw "$Name marker was not found at $Path." }
    $lines = @(Get-Content -LiteralPath $Path | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($lines.Count -ne 1) { throw "$Name marker must contain exactly one non-empty line." }
    $lines[0].Trim()
}

function ConvertFrom-Marker {
    param([string]$Marker, [string]$Prefix, [string]$Name)

    if (-not $Marker.StartsWith($Prefix, [System.StringComparison]::Ordinal)) { throw "$Name marker must start with '$Prefix'." }
    $version = $Marker.Substring($Prefix.Length)
    $parsed = $null
    if (-not [System.Version]::TryParse($version, [ref]$parsed)) { throw "$Name marker has an invalid semantic version: $Marker" }
    $version
}

function Get-Maps {
    param($Manifest)

    if ($Manifest -and $Manifest.map) { return @($Manifest.map) }
    if ($Manifest -and $Manifest.maps) { return @($Manifest.maps) }

    # Migrate the former URL-oriented manifest without carrying its URLs forward.
    $migrated = @()
    foreach ($guide in @($Manifest.guides)) {
        $migrated += [pscustomobject]@{
            guideVersion = [string]$guide.documentationVersion
            templateVersions = @($guide.supportedTemplateVersions)
        }
    }
    $migrated
}

$docsVersion = ConvertFrom-Marker (Read-SingleLine $DocsReleaseFile "Docs release") "docs-v" "Docs release"
$templateVersion = ConvertFrom-Marker (Read-SingleLine $TemplateReleaseFile "Template release") "template-v" "Template release"

$manifest = if (Test-Path -LiteralPath $ManifestPath) { Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json } else { $null }
$maps = @(Get-Maps $manifest)
$cleanMaps = @()

foreach ($map in $maps) {
    $versions = @($map.templateVersions) |
        ForEach-Object { [string]$_ } |
        Where-Object { $_ -and $_ -ne $templateVersion } |
        Sort-Object { [System.Version]::Parse($_) } -Unique

    if ($versions.Count -gt 0) {
        $cleanMaps += [pscustomobject]@{
            guideVersion = [string]$map.guideVersion
            templateVersions = @($versions)
        }
    }
}

$target = $cleanMaps | Where-Object { $_.guideVersion -eq $docsVersion } | Select-Object -First 1
if ($null -eq $target) {
    $target = [pscustomobject]@{ guideVersion = $docsVersion; templateVersions = @() }
    $cleanMaps += $target
}

$target.templateVersions = @($target.templateVersions + $templateVersion) |
    Sort-Object { [System.Version]::Parse($_) } -Unique

$orderedMaps = @($cleanMaps | Sort-Object { [System.Version]::Parse($_.guideVersion) })
$jsonLines = [System.Collections.Generic.List[string]]::new()
$null = $jsonLines.Add('{')
$null = $jsonLines.Add('  "map": [')
for ($index = 0; $index -lt $orderedMaps.Count; $index++) {
    $map = $orderedMaps[$index]
    $comma = if ($index -lt $orderedMaps.Count - 1) { ',' } else { '' }
    $null = $jsonLines.Add('    {')
    $null = $jsonLines.Add(('      "guideVersion": "{0}",' -f $map.guideVersion))
    $null = $jsonLines.Add('      "templateVersions": [')
    for ($versionIndex = 0; $versionIndex -lt $map.templateVersions.Count; $versionIndex++) {
        $versionComma = if ($versionIndex -lt $map.templateVersions.Count - 1) { ',' } else { '' }
        $null = $jsonLines.Add(('        "{0}"{1}' -f $map.templateVersions[$versionIndex], $versionComma))
    }
    $null = $jsonLines.Add(('      ]'))
    $null = $jsonLines.Add(('    }}{0}' -f $comma))
}
$null = $jsonLines.Add('  ]')
$null = $jsonLines.Add('}')
$json = $jsonLines -join [Environment]::NewLine
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $ManifestPath), $json + [Environment]::NewLine, $utf8NoBom)
