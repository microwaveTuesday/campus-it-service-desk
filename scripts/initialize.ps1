$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
if (Test-Path .env) { throw '.env already exists; retain your existing configuration.' }
$dbSecret = [guid]::NewGuid().ToString('N')
$jwtSecret = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
"POSTGRES_PASSWORD=$dbSecret`nJWT_KEY=$jwtSecret" | Set-Content .env
Write-Host 'Created local secrets in .env. Start with docker compose up --build -d.'
