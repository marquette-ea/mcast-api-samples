# This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
$apiKey = "$env:MCAST_API_KEY"

# Change this to your company's unique MCast domain
$mcastDomain = "demo-gas.mea-analytics.tools"

# This will just test the API call without making any changes.
# Change this to "false" to make real changes to your MCast database.
$dryRun = "true"

$body = @()

foreach ($day in (25 .. 31)) {
  $date = New-Object System.DateTime 2022,07,$day
  
  $body += @{
    operatingArea = "Metropolis";
    date = $date.ToShortDateString();
    load = $day*100;
  }

  $body += @{
    operatingArea = "Smallville";
    date = $date.ToShortDateString();
    load = $day*10;
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
