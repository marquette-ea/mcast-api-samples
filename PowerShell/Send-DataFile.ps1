# This expects to find an evironment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
$apiKey = "$env:MCAST_API_KEY"

# Change this to your company's unique MCast domain
$mcastDomain = "demo-gas.mea-analytics.tools"

# This will just test the API call without making any changes.
# Change this to "false" to make real changes to your MCast database.
$dryRun = "true"

$file = Get-Item -Path "data.csv"

$response = Invoke-WebRequest `
  -Method "POST" `
  -Uri "https://$mcastDomain/api/v1/upload-file?dryRun=$dryRun" `
  -Headers @{ "x-api-key" = $apiKey } `
  -ContentType "multipart/form-data" `
  -Form @{ file = $file; type = "text/csv" }
