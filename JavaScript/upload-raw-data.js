// This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
// You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
const apiKey = process.env.MCAST_API_KEY;

if (!apiKey) {
  throw "Unable to find MCAST_API_KEY in environment variables; please check and ensure this key is present!"
}

// Change this to your company's unique MCast domain
const mcastDomain = "demo-gas.mea-analytics.tools";

// This will just test the API call without making any changes.
// Change this to "false" to make real changes to your MCast database.
const dryRun = "true";

// Replace these specific values with the values you wish to upload; 
// Note constructing Dates in Javascript uses values of 0-11 for 

var dates = Array.from({length:7}, (_,i) => new Date(2022, 9, i))
var smallvilleLoad = [1234, 2345, 3456, 4567, 5678, 6789, 7890]
var metropolisLoad = [4321, 5432, 6543, 7654, 8765, 9876, 0987] 



function formatDate(date) {
  var year = date.getFullYear()
  var adjustedMonth = date.getMonth()+1 
  var month = (adjustedMonth < 10) ? `0${adjustedMonth}` : `${adjustedMonth}`
  var day = (date.getDate() < 10) ? `0${date.getDate()}` : `${date.getDate()}`
  return `${year}-${month}-${day}`
}

async function uploadData(uri = '', data = {}) {
  const headers = 
    { method: "POST",
      headers: {
        "x-api-key": apiKey,
        "accept": "*/*",
        "Content-Type" : "application/json"
      },
      body: data
    } 
  var response = await fetch(uri, headers)
  return response.json()
} 


var json = []

for (let i = 0; i < dates.length; i++) {
  
  metropolisElement = {
    "operatingArea": "Metropolis",
    "date": formatDate(dates[i]),
    "load": metropolisLoad[i]
  }
  json.push(metropolisElement)
  
  smallvilleElement = {
    "operatingArea": "Smallville",
    "date": formatDate(dates[i]),
    "load": smallvilleLoad[i]
  }
  json.push(smallvilleElement)
}

jsonString = JSON.stringify(json)
const uri = `https://${mcastDomain}/api/v1/daily/observed-load?dryRun=${dryRun}`

uploadData(uri, jsonString).then((data) => console.log(data))
