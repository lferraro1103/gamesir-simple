# Arranges the published app into the portable layout:
#   GameSirSimple.exe
#   docs/      READMEs, CHANGELOG, VALIDATION.txt and screenshots
#   licenses/  app license, third-party notices and upstream licenses
# Relative Markdown links are rewritten so they keep working inside the package.
param(
    [string]$Dist = 'dist/GameSirSimple',
    [string]$Checks = 'artifacts/visual/visual-check.txt'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$utf8 = [System.Text.UTF8Encoding]::new($false)

$docs = Join-Path $Dist 'docs'
$licenses = Join-Path $Dist 'licenses'
New-Item -ItemType Directory -Force $docs, $licenses, (Join-Path $docs 'images') | Out-Null

foreach ($name in 'README.md', 'README.es.md', 'README.zh-CN.md', 'CHANGELOG.md') {
    $text = [System.IO.File]::ReadAllText((Join-Path $root $name))
    $text = $text.Replace('](docs/images/', '](images/').Replace('](LICENSE)', '](../licenses/LICENSE.txt)')
    $text = $text.Replace('](THIRD_PARTY.md)', '](../licenses/THIRD_PARTY.md)').Replace('](licenses)', '](../licenses)')
    [System.IO.File]::WriteAllText((Join-Path $docs $name), $text, $utf8)
}
Copy-Item (Join-Path $root 'docs/images/*') (Join-Path $docs 'images')
if (Test-Path $Checks) { Copy-Item $Checks (Join-Path $docs 'VALIDATION.txt') }

Copy-Item (Join-Path $root 'LICENSE') (Join-Path $licenses 'LICENSE.txt')
Copy-Item (Join-Path $root 'licenses/*') $licenses
$thirdParty = [System.IO.File]::ReadAllText((Join-Path $root 'THIRD_PARTY.md')).Replace('](licenses/', '](')
[System.IO.File]::WriteAllText((Join-Path $licenses 'THIRD_PARTY.md'), $thirdParty, $utf8)
