
import os
import requests
from datetime import date
from dataclasses import dataclass
import json

# This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
api_key = os.environ["MCAST_API_KEY"]

# Change this to your company's unique MCast domain
mcast_domain = "demo-gas.mea-analytics.tools"

# This will just test the API call without making any changes.
# Change this to False to make real changes to your MCast database.
dry_run = True

dates = [ date(2022,7,day) for day in range(25, 32) ]

metropolisLoad = [ 134537, 135647, 136389, 137446, 132354, 124888, 127618 ]
smallvilleLoad = [ 33409, 33033, 32765, 34437, 33164, 31412, 30426 ]

@dataclass(frozen=True)
class PostJson:
  operating_area: str
  date: date
  load: float

  def to_dict(self):
    return { 
      "operatingArea": self.operating_area,
      "date": self.date.isoformat(),
      "load": self.load
    }

loads = []
for i in range(0, 7):
  loads.append(
    PostJson(
      operating_area = "Metropolis",
      date = dates[i],
      load = metropolisLoad[i]
    )
  )

  loads.append(
    PostJson(
      operating_area = "Smallville",
      date = dates[i],
      load = smallvilleLoad[i]
    )
  )

print("Submitting load values...")

params = { "dryRun": dry_run }
headers = { "x-api-key": api_key, "content-type": "application/json" }
body = json.dumps([ load.to_dict() for load in loads ])

response = requests.post(f"https://{mcast_domain}/api/v1/daily/observed-load", params=params, headers=headers, data=body)
print(response)
if not response.ok: print(response.text)
response.raise_for_status()

print()
print("Generating a forecast...")


headers = { "x-api-key": api_key }
response = requests.post(f"https://{mcast_domain}/api/v1/daily/generate-forecast", params=params, headers=headers)
print(response)
print(response.text)
response.raise_for_status()
