#!/bin/bash

# This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
apiKey="$MCAST_API_KEY"

# Change this to your company's unique MCast domain
mcastDomain="demo-gas.mea-analytics.tools"

# This will just test the API call without making any changes.
# Change this to false to make real changes to your MCast database.
dryRun=true

json=()
for d in {25..31}
do
  date="2022-07-$d"
  metLoad=$((d*100))
  smlLoad=$((d*10))
  metJson=$(jq -nc --arg date $date --argjson load $metLoad '{"operatingArea":"Metropolis","date":$date,"load":$load}')
  json+=("$metJson")

  smlJson=$(jq -nc --arg date $date --argjson load $smlLoad '{"operatingArea":"Smallville","date":$date,"load":$load}')
  json+=("$smlJson")
done

body=$(jq -n '$ARGS.positional' --jsonargs ${json[@]})

printf "submitting load values...\n"
curl -iX "POST" \
  "https://$mcastDomain/api/v1/daily/observed-load?dryRun=$dryRun" \
  -H "accept: */*" \
  -H "x-api-key: $apiKey" \
  -H "Content-Type: application/json" \
  -d "$body"

printf "\n\n"
printf "Generating a forecast...\n"

curl -iX "POST" \
  "https://$mcastDomain/api/v1/daily/generate-forecast?dryRun=$dryRun" \
  -H "accept: */*" \
  -H "x-api-key: $apiKey" \
