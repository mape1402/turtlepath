param(
    [string]$DocsRoot = "docs/guides/template",
    [string]$DocsReleaseFile = ".docs.release",
    [string]$TemplateReleaseFile = ".template.release",
    [string]$RepositoryRawBase = "https://raw.githubusercontent.com/mape1402/turtlepath"
)

$ErrorActionPreference = "Stop"

function Read-SingleLine {
    param(
        [string]$Path,
        [string]$Name
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Name marker was not found at $Path."
    }

    $lines = @(Get-Content -LiteralPath $Path | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($lines.Count -ne 1) {
        throw "$Name marker must contain exactly one non-empty line."
    }

    return $lines[0].Trim()
}

function ConvertFrom-Marker {
    param(
        [string]$Marker,
        [string]$Prefix,
        [string]$Name
    )

    if (-not $Marker.StartsWith($Prefix, [System.StringComparison]::Ordinal)) {
        throw "$Name marker must start with '$Prefix'. Current value: $Marker"
    }

    $version = $Marker.Substring($Prefix.Length)
    $parsedVersion = $null
    if (-not [System.Version]::TryParse($version, [ref]$parsedVersion)) {
        throw "$Name marker has an invalid semantic version. Current value: $Marker"
    }

    return $version
}

function Get-NextMinorUpperBound {
    param([string[]]$Versions)

    $parsedVersions = $Versions |
        ForEach-Object { [System.Version]::Parse($_) } |
        Sort-Object Major, Minor, Build

    $highest = $parsedVersions[-1]
    return "$($highest.Major).$($highest.Minor + 1).0"
}

$docsMarker = Read-SingleLine -Path $DocsReleaseFile -Name "Docs release"
$docsVersion = ConvertFrom-Marker -Marker $docsMarker -Prefix "docs-v" -Name "Docs release"

$templateMarker = Read-SingleLine -Path $TemplateReleaseFile -Name "Template release"
$templateVersion = ConvertFrom-Marker -Marker $templateMarker -Prefix "template-v" -Name "Template release"

$guideId = "template-use-guide-$docsVersion"
$guideDirectory = Join-Path $DocsRoot $guideId
$englishGuide = Join-Path $guideDirectory "en.md"
$spanishGuide = Join-Path $guideDirectory "es.md"
$manifestPath = Join-Path $DocsRoot "guide-manifest.json"

foreach ($requiredFile in @($englishGuide, $spanishGuide)) {
    if (-not (Test-Path -LiteralPath $requiredFile)) {
        throw "Required guide file was not found: $requiredFile"
    }
}

if (Test-Path -LiteralPath $manifestPath) {
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $guides = @($manifest.guides)
}
else {
    $guides = @()
}

$existingGuide = $guides | Where-Object { $_.documentationVersion -eq $docsVersion } | Select-Object -First 1
$supportedVersions = @()
if ($existingGuide -and $existingGuide.supportedTemplateVersions) {
    $supportedVersions += @($existingGuide.supportedTemplateVersions)
}

$supportedVersions += $templateVersion
$supportedVersions = $supportedVersions |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Sort-Object { [System.Version]::Parse($_) } -Unique

$minimumVersion = $supportedVersions[0]
$upperBound = Get-NextMinorUpperBound -Versions $supportedVersions
$range = "[$minimumVersion,$upperBound)"
$rawBase = $RepositoryRawBase.TrimEnd("/")
$sourceRef = "docs-v$docsVersion"

$guide = [ordered]@{
    id = $guideId
    title = "TurtlePath Template Use Guide"
    documentationVersion = $docsVersion
    packageId = "TurtlePath.Template"
    supportedTemplateVersionRange = $range
    supportedTemplateVersions = @($supportedVersions)
    cultures = @(
        [ordered]@{
            code = "en"
            title = "English"
            sourceUrl = "$rawBase/$sourceRef/docs/guides/template/$guideId/en.md"
        },
        [ordered]@{
            code = "es"
            title = "Espanol"
            sourceUrl = "$rawBase/$sourceRef/docs/guides/template/$guideId/es.md"
        }
    )
    source = "GitHub"
}

$updatedGuides = @($guides | Where-Object { $_.documentationVersion -ne $docsVersion })
$updatedGuides += [pscustomobject]$guide
$updatedGuides = $updatedGuides | Sort-Object { [System.Version]::Parse($_.documentationVersion) } -Descending

$updatedManifest = [ordered]@{
    guides = @($updatedGuides)
}

$json = $updatedManifest | ConvertTo-Json -Depth 20
Set-Content -LiteralPath $manifestPath -Value $json -Encoding utf8
