<#
.SYNOPSIS
  Saves the Anthropic key for the public demo into ~/.tower/secrets.json as Paragraf:AnthropicApiKey.
.DESCRIPTION
  The demo should have its own key, with a spend limit set in the Anthropic console, so a public page can never
  spend more than that. The key is typed into a hidden prompt and is never printed. Run it yourself in a terminal;
  then scripts/deploy-demo.ps1 -Secrets copies it to the server.
.EXAMPLE
  .\scripts\set-demo-key.ps1
#>
$ErrorActionPreference = "Stop"
$target = Join-Path $HOME ".tower\secrets.json"
New-Item -ItemType Directory -Force (Split-Path $target) | Out-Null
$secrets = [ordered]@{}
if (Test-Path $target) {
    (Get-Content $target -Raw | ConvertFrom-Json).PSObject.Properties | ForEach-Object { $secrets[$_.Name] = $_.Value }
}
$secure = Read-Host "Anthropic key for the paragraf demo (hidden)" -AsSecureString
$plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
if (-not $plain.StartsWith("sk-ant-")) { throw "That does not look like an Anthropic key (sk-ant-...). Nothing saved." }
$secrets["Paragraf:AnthropicApiKey"] = $plain
[IO.File]::WriteAllText($target, ($secrets | ConvertTo-Json), (New-Object Text.UTF8Encoding $false))
Write-Host "Saved Paragraf:AnthropicApiKey in $target. Next: .\scripts\deploy-demo.ps1 -Secrets" -ForegroundColor Green
