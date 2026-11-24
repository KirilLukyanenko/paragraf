<#
.SYNOPSIS
  What the public demo was asked: copies the ask log from the server and prints `paragraf asks` for it.
.DESCRIPTION
  The log (questions, answers, statuses, costs, visitors' marks) is in the demo's database on the ArbBot VM and is
  kept there for 30 days. This script exports the last -Days days as JSON lines into evals/feedback/ (git-ignored:
  visitors' questions never go into the public repository), prints the summary and writes the misses as unlabelled
  eval questions next to it. Read, anonymise and label them before adding any to evals/holdout.jsonl.
.EXAMPLE
  .\scripts\demo-asks.ps1
  .\scripts\demo-asks.ps1 -Days 7
#>
param(
    [int] $Days = 30,
    [string] $Project = "arbbot-xgp5f8",
    [string] $Zone = "europe-central2-a",
    [string] $Instance = "arbbot"
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root "evals\feedback"
New-Item -ItemType Directory -Force $out | Out-Null
$stamp = Get-Date -Format "yyyyMMdd-HHmm"
$file = Join-Path $out "asks-$stamp.jsonl"
$candidates = Join-Path $out "candidates-$stamp.jsonl"

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

# No double quotes in the remote command: Windows PowerShell 5.1 mangles them on the way to gcloud.
$sql = "select row_to_json(a) from asks a where at > now() - make_interval(days => $Days) order by at"
$remote = "cd ~/paragraf && sudo docker compose -p paragraf -f deploy/docker-compose.yml exec -T db psql -U paragraf -d paragraf -At -c '$sql' > ~/paragraf-asks.jsonl"
Write-Host "Exporting the last $Days days of asks on the server…"
Invoke-GCloud @("compute", "ssh", $Instance, "--project=$Project", "--zone=$Zone", "--quiet", "--command=$remote") "export the asks"
Invoke-GCloud @("compute", "scp", "--project=$Project", "--zone=$Zone", "--quiet", "${Instance}:~/paragraf-asks.jsonl", $file) "copy the asks"
Invoke-GCloud @("compute", "ssh", $Instance, "--project=$Project", "--zone=$Zone", "--quiet", "--command=rm -f ~/paragraf-asks.jsonl") "remove the copy on the server"

Write-Host "Saved to $file"
& dotnet run --project (Join-Path $root "src\Paragraf.Cli") -- asks --file $file --days $Days --export $candidates
if ($LASTEXITCODE -ne 0) { throw "paragraf asks failed" }
