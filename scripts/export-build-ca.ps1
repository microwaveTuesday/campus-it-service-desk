$ErrorActionPreference = 'Stop'
# Export public certificates already trusted by Windows; no private keys are exported.
$targetPath = Join-Path (Split-Path $PSScriptRoot -Parent) 'frontend/build-ca.pem'
$pemLines = Get-ChildItem Cert:\LocalMachine\Root,Cert:\CurrentUser\Root,Cert:\LocalMachine\CA,Cert:\CurrentUser\CA | ForEach-Object {
    '-----BEGIN CERTIFICATE-----'
    [Convert]::ToBase64String($_.RawData,[Base64FormattingOptions]::InsertLineBreaks)
    '-----END CERTIFICATE-----'
}
$pemLines | Set-Content -LiteralPath $targetPath -Encoding ascii
Write-Host "Public CA bundle exported to $targetPath. Set BUILD_CA_FILE to this path and use docker-compose.trust.yml."

