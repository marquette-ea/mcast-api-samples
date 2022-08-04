#!/bin/bash

# This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
apiKey="$MCAST_API_KEY"

# Change this to your company's unique MCast domain
mcastDomain="demo-gas.mea-analytics.tools"

# This will just test the API call without making any changes.
# Change this to false to make real changes to your MCast database.
dryRun=true

dates=()
for d in {25..31}; do dates+=("2022-07-$d"); done
metropolisLoads=(134537 135647 136389 137446 132354 124888 127618)
smallvilleLoads=(33409 33033 32765 34437 33164 31412 30426)
json=()
for i in {0..6}
do
  date=${dates[$i]}
  metropolisLoad=${metropolisLoads[$i]}
  smallvilleLoad=${smallvilleLoads[$i]}

  metropolisJson=$(jq -nc --arg date $date --argjson load $metropolisLoad '{"operatingArea":"Metropolis","date":$date,"load":$load}')
  json+=("$metropolisJson")

  smallvilleJson=$(jq -nc --arg date $date --argjson load $smallvilleLoad '{"operatingArea":"Smallville","date":$date,"load":$load}')
  json+=("$smallvilleJson")
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
  -H "x-api-key: $apiKey"
