
function formatDate(date) {
  var year = date.getFullYear()
  var adjustedMonth = date.getMonth()+1 
  var month = (adjustedMonth < 10) ? `0${adjustedMonth}` : `${adjustedMonth}`
  var day = (date.getDate() < 10) ? `0${date.getDate()}` : `${date.getDate()}`
  return `${year}-${month}-${day}`
}

// This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
// You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
const api_key = process.env.MCAST_API_KEY;
//if (!api_key) {
//  throw "Unable to find MCAST_API_KEY in environment variables; please check and ensure this key is present!"
//}

async function uploadData(uri = '', data = {}) {
  const headers = 
    { method: "POST",
      headers: {
        "x-api-key": api_key,
        "accept": "*/*",
        "Content-Type" : "application/json"
      },
      body: data
    } 
  var response = await fetch(uri, headers)
  return response.json()
} 

// Change this to your company's unique MCast domain
const mcast_domain = "demo-gas.mea-analytics.tools";

// This will just test the API call without making any changes.
// Change this to "false" to make real changes to your MCast database.
const dry_run = "true";

var json = []

var dates = Array.from({length:7}, (_,i) => new Date(2022, 9, i))
var smallville_load = [1234, 2345, 3456, 4567, 5678, 6789, 7890]
var metropolis_load = [4321, 5432, 6543, 7654, 8765, 9876, 0987] 

for (let i = 0; i < dates.length; i++) {
  
  metropolis_element = {
    "operatingArea": "Metropolis",
    "date": formatDate(dates[i]),
    "load": metropolis_load[i]
  }
  json = json.concat(metropolis_element)
  
  smallville_element = {
    "operatingArea": "Smallville",
    "date": formatDate(dates[i]),
    "load": smallville_load[i]
  }
  json = json.concat(smallville_element)
}

json_string = JSON.stringify(json)
const uri = `https://${mcast_domain}/api/v1/daily/observed-load?dryRun=${dry_run}`

uploadData(uri, json_string).then((data) => console.log(data))
