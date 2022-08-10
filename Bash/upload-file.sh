#!/bin/bash

# This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
apiKey="$MCAST_API_KEY"

# Change this to your company's unique MCast domain
mcastDomain="demo-gas.mea-analytics.tools"

# This will just test the API call without making any changes.
# Change this to false to make real changes to your MCast database.
dryRun=true

filePath="data.csv"

curl -iX "POST" \
  "https://$mcastDomain/api/v1/upload-file?dryRun=$dryRun" \
  -H "accept: */*" \
  -H "x-api-key: $apiKey" \
  -H "Content-Type: multipart/form-data" \
  -F "filename=@$filePath;type=text/csv"
