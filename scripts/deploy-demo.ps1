<#
.SYNOPSIS
  The public paragraf demo on the ArbBot VM: paragraf, pgvector, comp-ledger with the demo employees, behind the
  VM's Caddy at https://paragraf.<ip>.sslip.io. Running it again updates the code.
.DESCRIPTION
  1. Code: archives of the committed HEAD of paragraf and of comp-ledger (../comp-ledger), both of which must be on
     GitHub already, are copied to ~/paragraf on the server. Nothing uncommitted is sent.
  2. -Secrets: copies Paragraf:AnthropicApiKey (the demo's own key, see scripts/set-demo-key.ps1) and OpenAI:ApiKey
     from ~/.tower/secrets.json into ~/paragraf/deploy/.env on the server (mode 600). Values are never printed.
     Run by the owner, not by Claude.
  3. deploy/bootstrap.sh on the server: database passwords on the first run, comp-ledger and its demo tenant once,
     then paragraf, which loads the code into pgvector on its first start.
  4. The address: on the first run, PARAGRAF_DOMAIN goes into the bot's deploy/.env and only Caddy restarts. The
     bot's Caddyfile must already have the paragraf block (ArbBot master).
  One deployment at a time on the server: the same lock as Tower's deploy-server.ps1.
.EXAMPLE
  .\scripts\deploy-demo.ps1 -Secrets                     # the first time
  .\scripts\deploy-demo.ps1                              # update the code
  .\scripts\deploy-demo.ps1 -Model claude-sonnet-5-5     # change the answer model
#>
param(
    [switch] $Secrets,
    [switch] $AllowUnpushed,
    [string] $Model,
    [string] $Project = "arbbot-xgp5f8",
    [string] $Zone = "europe-central2-a",
    [string] $Instance = "arbbot",
    [string] $Domain = "paragraf.34-116-158-217.sslip.io"
)
$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$root = Split-Path $PSScriptRoot -Parent
$compLedger = Join-Path (Split-Path $root -Parent) "comp-ledger"
$ssh = @("compute", "ssh", $Instance, "--project=$Project", "--zone=$Zone", "--quiet")
$scp = @("compute", "scp", "--project=$Project", "--zone=$Zone", "--quiet")
$work = Join-Path ([IO.Path]::GetTempPath()) "paragraf-deploy"
New-Item -ItemType Directory -Force $work | Out-Null

# gcloud and git write progress to stderr; with Stop, Windows PowerShell 5.1 turns that into an exception.
$GcloudExe = (Get-Command gcloud -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
function Invoke-GCloud([string[]] $Arguments, [string] $What) {
    $prev = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & $GcloudExe @Arguments 2>&1 | ForEach-Object { Write-Host "    $_" }
        $code = $LASTEXITCODE
    } finally { $ErrorActionPreference = $prev }
    if ($code -ne 0) { throw "Failed ($What), exit code $code" }
}
function Remote([string] $Command, [string] $What = "command on the server") {
    Invoke-GCloud ($ssh + "--command=$Command") $What
}

# Archives the committed HEAD of a repository, which must be on GitHub (origin's default branch).
function Get-Archive([string] $Repo, [string] $Name, [string] $Prefix) {
    if (-not (Test-Path (Join-Path $Repo ".git"))) { throw "No repository at $Repo" }
    $prev = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $head = (git -C $Repo rev-parse --short HEAD 2>$null).Trim()
        git -C $Repo fetch -q origin 2>$null
        $default = (git -C $Repo rev-parse --abbrev-ref origin/HEAD 2>$null).Trim()
        if (-not $default) { $default = "origin/main" }
        git -C $Repo merge-base --is-ancestor HEAD $default 2>$null
        if ($LASTEXITCODE -ne 0 -and -not $AllowUnpushed) {
            throw "$Name $head is not on GitHub ($default). Push first, then deploy."
        }
        $tar = Join-Path $work "$Name.tar"
        $prefixArg = if ($Prefix) { "--prefix=$Prefix" } else { $null }
        git -C $Repo archive --format=tar $prefixArg -o $tar HEAD 2>$null
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $tar)) { throw "git archive failed for $Name" }
        return @{ Tar = $tar; Head = $head }
    } finally { $ErrorActionPreference = $prev }
}

$paragraf = Get-Archive $root "paragraf" $null
$ledger = Get-Archive $compLedger "comp-ledger" "comp-ledger/"
Write-Host "paragraf $($paragraf.Head), comp-ledger $($ledger.Head)"

# The deployment lock shared with Tower: two deployments at once on one server broke builds before.
$lockTaken = $false
for ($i = 0; -not $lockTaken; $i++) {
    $prev = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $who = "paragraf-$($paragraf.Head)"
        $answer = & $GcloudExe @($ssh + "--command=if mkdir ~/tower-deploy.lock 2>/dev/null; then echo $who > ~/tower-deploy.lock/who; echo TAKEN; exit 0; fi; age=`$(( `$(date +%s) - `$(stat -c %Y ~/tower-deploy.lock) )); if [ `$age -gt 1200 ]; then rm -rf ~/tower-deploy.lock; mkdir ~/tower-deploy.lock; echo $who > ~/tower-deploy.lock/who; echo TAKEN; exit 0; fi; echo BUSY `$(cat ~/tower-deploy.lock/who 2>/dev/null) `$age") 2>&1 | ForEach-Object { "$_" }
    } finally { $ErrorActionPreference = $prev }
    if ($answer -match 'TAKEN') { $lockTaken = $true; break }
    $busy = ($answer | Where-Object { $_ -match '^BUSY' } | Select-Object -First 1)
    if (-not $busy) { throw "Could not take the deployment lock on the server: $($answer -join ' ')" }
    if ($i -ge 30) { throw "Another deployment has held the server for over 10 minutes ($busy)." }
    Write-Host "Another deployment is running ($busy s); waiting..." -ForegroundColor Yellow
    Start-Sleep -Seconds 20
}
try {
    Write-Host "Code -> server..."
    Invoke-GCloud ($scp + @($paragraf.Tar, "${Instance}:paragraf.tar")) "copying paragraf"
    Invoke-GCloud ($scp + @($ledger.Tar, "${Instance}:comp-ledger.tar")) "copying comp-ledger"
    # The new code next to the old, deploy/.env carried over, then the swap.
    Remote 'set -e; rm -rf ~/paragraf.new; mkdir ~/paragraf.new; tar -xf ~/paragraf.tar -C ~/paragraf.new; tar -xf ~/comp-ledger.tar -C ~/paragraf.new; rm ~/paragraf.tar ~/comp-ledger.tar; if [ -f ~/paragraf/deploy/.env ]; then cp -p ~/paragraf/deploy/.env ~/paragraf.new/deploy/.env; fi; rm -rf ~/paragraf; mv ~/paragraf.new ~/paragraf' "unpacking"

    if ($Secrets) {
        $file = Join-Path $HOME ".tower\secrets.json"
        if (-not (Test-Path $file)) { throw "No $file" }
        $json = Get-Content $file -Raw | ConvertFrom-Json
        if (-not $json.'Paragraf:AnthropicApiKey') { throw "No Paragraf:AnthropicApiKey in $file. Run .\scripts\set-demo-key.ps1 first." }
        if (-not $json.'OpenAI:ApiKey') { throw "No OpenAI:ApiKey in $file." }
        $tmp = Join-Path $work "paragraf.secrets"
        try {
            $lines = "ANTHROPIC_API_KEY=$($json.'Paragraf:AnthropicApiKey')`nOPENAI_API_KEY=$($json.'OpenAI:ApiKey')`n"
            [IO.File]::WriteAllText($tmp, $lines, (New-Object Text.UTF8Encoding $false))
            icacls $tmp /inheritance:r /grant:r "${env:USERNAME}:(F)" | Out-Null
            Invoke-GCloud ($scp + @($tmp, "${Instance}:paragraf.secrets")) "copying the keys"
            Remote 'set -e; chmod 600 ~/paragraf.secrets; cd ~/paragraf/deploy; touch .env; chmod 600 .env; grep -v -e ^ANTHROPIC_API_KEY= -e ^OPENAI_API_KEY= .env > .env.new || true; tr -d ''\r'' < ~/paragraf.secrets >> .env.new; rm ~/paragraf.secrets; chmod 600 .env.new; mv .env.new .env' "writing the keys"
        } finally { Remove-Item $tmp -Force -ErrorAction SilentlyContinue }
        Write-Host "Keys on the server: ANTHROPIC_API_KEY (the demo key), OPENAI_API_KEY"
    }
    if ($Model) {
        if ($Model -notmatch '^claude-[a-z0-9-]+$') { throw "Not a model id: $Model" }
        Remote "set -e; cd ~/paragraf/deploy; touch .env; grep -v -e ^PARAGRAF_MODEL= .env > .env.new || true; echo PARAGRAF_MODEL=$Model >> .env.new; chmod 600 .env.new; mv .env.new .env" "setting the model"
    }

    Write-Host "Building and starting on the server (the first run takes a few minutes)..."
    Remote 'set -e; cd ~/paragraf && sudo sh deploy/bootstrap.sh' "bootstrap"

    # The address at the bot's Caddy, once: the bot's repository must have the paragraf block in its Caddyfile.
    Remote "set -e; if grep -q '^PARAGRAF_DOMAIN=' ~/arbbot/deploy/.env; then exit 0; fi; cd ~/arbbot; git pull --ff-only -q; grep -q PARAGRAF_DOMAIN deploy/Caddyfile || { echo 'The bot Caddyfile has no paragraf block: merge it into ArbBot master first'; exit 1; }; echo 'PARAGRAF_DOMAIN=$Domain' >> deploy/.env; sudo docker compose -f deploy/docker-compose.yml up -d --no-deps caddy; sudo docker compose -f deploy/docker-compose.yml restart caddy" "address at Caddy"
} finally {
    Remote 'rm -rf ~/tower-deploy.lock' "releasing the lock"
}

# From outside: the first start loads the code into pgvector, and Caddy issues the certificate on the first request.
Write-Host "Checking https://$Domain/health ..."
for ($i = 0; $i -lt 48; $i++) {
    try {
        $r = Invoke-WebRequest "https://$Domain/health" -UseBasicParsing -TimeoutSec 10
        if ($r.StatusCode -eq 200) { Write-Host "The demo works: https://$Domain (paragraf $($paragraf.Head), comp-ledger $($ledger.Head))" -ForegroundColor Green; exit 0 }
    } catch { Start-Sleep -Seconds 5 }
}
throw "https://$Domain/health did not answer in 4 minutes. Logs: gcloud compute ssh $Instance --zone=$Zone --command=`"cd paragraf && sudo docker compose -f deploy/docker-compose.yml logs --tail=80`""
