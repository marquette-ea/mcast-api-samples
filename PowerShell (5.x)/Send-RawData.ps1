$ErrorActionPreference = 'Stop'

# This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
$apiKey = "$env:MCAST_API_KEY"

# Change this to your company's unique MCast domain
$mcastDomain = "demo-gas.mea-analytics.tools"

# This will just test the API call without making any changes.
# Change this to "false" to make real changes to your MCast database.
$dryRun = "true"

$dates = @()
foreach ($day in (25 .. 31)) { $dates += New-Object System.DateTime 2022,07,$day }
$metropolisLoad = @(134537, 135647, 136389, 137446, 132354, 124888, 127618)
$smallvilleLoad = @(33409, 33033, 32765, 34437, 33164, 31412, 30426)
$body = @()
foreach ($i in (0 .. 6)) {
  
  $body += @{
    operatingArea = "Metropolis";
    date = $dates[$i].ToShortDateString();
    load = $metropolisLoad[$i];
  }

  $body += @{
    operatingArea = "Smallville";
    date = $dates[$i].ToShortDateString();
    load = $smallvilleLoad[$i];
  }
}

Write-Host "submitting load values..."
Invoke-WebRequest `
  -Method "POST" `
  -Uri "https://$mcastDomain/api/v1/daily/observed-load?dryRun=$dryRun" `
  -Headers @{ "x-api-key" = $apiKey } `
  -ContentType "application/json" `
  -Body (ConvertTo-Json $body)

Write-Host ""
Write-Host "Generating a forecast..."

Invoke-WebRequest `
  -Method "POST" `
  -Uri "https://$mcastDomain/api/v1/daily/generate-forecast?dryRun=$dryRun" `
  -Headers @{ "x-api-key" = $apiKey }
