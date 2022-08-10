
// This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
// You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
const api_key = process.env.MCAST_API_KEY;
if (!api_key) {
  throw "Unable to find MCAST_API_KEY in environment variables; please check and ensure this key is present!"
}

// Change this to your company's unique MCast domain
const mcast_domain = "demo-gas.mea-analytics.tools";

let maxDate = new Date()
maxDate.setHours(0,0,0,0)

let minDate = new Date(maxDate.getTime())
minDate.setDate(maxDate.getDate() - 60)

const opArea = "Metropolis"
const idf = 1

// "horizon" means how many days ahead the forecast is looking.  Each MCast forecast has horizons 0 through 7, meaning
// it forecasts "today" through "next week" (7 days from "today").  This report will examine the MAPE of forecasts made
// for horizon 1, so looking at how accurate the forecasts are for "tomorrow".
forecastHorizon = 1

function formatDate(date) {
  var year = date.getFullYear()
  var adjustedMonth = date.getMonth()+1 
  var month = (adjustedMonth < 10) ? `0${adjustedMonth}` : `${adjustedMonth}`
  var day = (date.getDate() < 10) ? `0${date.getDate()}` : `${date.getDate()}`
  return `${year}-${month}-${day}`
}

async function getJsonRequest(uri, queryParams) {

  const query = new URLSearchParams(queryParams).toString()

  const requestParams = { method: "GET", headers: { "x-api-key": api_key } }

  const response = await fetch(`${uri}?${query}`, requestParams)
  console.log(`Status code: ${response.status}`)
  if (response.status >= 300) { 
    const msg = await response.text()
    throw `API call failed with status code ${response.status}: ${msg}`
  }
  return response.json()
}

// This retrieves the raw data from the MCast API that we will need in order to compute the MAPE by weekday 
async function getDataViaApi() {

  console.log("Retrieving forecast data...")

  const fcstParams = {  
    operatingArea: opArea,
    startDate: formatDate(minDate),
    endDate: formatDate(maxDate),
    idf: idf
  }
  
  const fcsts = await getJsonRequest(`https://${mcast_domain}/api/v1/daily/forecasted-load`, fcstParams)

  console.log("Retrieving observed data...")

  const observedParams = {  
    operatingArea: opArea,
    startDate: formatDate(minDate),
    endDate: formatDate(maxDate)
  }

  const observations = await getJsonRequest(`https://${mcast_domain}/api/v1/daily/observed-load`, observedParams)

  return {observations, fcsts}
}

// This takes the data we've already retrieved from the MCast API and computes the MAPE score for each weekday,
// printing the results out to the console.
function createReport(observations, fcsts) {

  let tomorrowForecasts = []
  for (let fcst of fcsts) {
    for (let fcstDay of fcst.loadForecast) {
      if (fcstDay.daysOut === forecastHorizon) tomorrowForecasts.push(fcstDay)
    }
  }
  
  let fcstsByDate = new Map()
  for (let fcst of tomorrowForecasts) {
    fcstsByDate.set(fcst.date, fcst.forecast)
  }

  let observationsByDate = new Map()
  for (let obs of observations) {
    observationsByDate.set(obs.date, obs.netLoad)
  }

  let dateRange = []
  let date = new Date(minDate.getTime())
  while (date.getTime() <= maxDate.getTime()) {
    dateRange.push(date)
    date = new Date(date.getTime())
    date.setDate(date.getDate() + 1)
  }

  let absolutePercentError = []
  for (let date of dateRange) {
    if (observationsByDate.has(formatDate(date)) && fcstsByDate.has(formatDate(date))) {
      let error = Math.abs((fcstsByDate.get(formatDate(date)) - observationsByDate.get(formatDate(date))) / observationsByDate.get(formatDate(date)))
      absolutePercentError.push({ date, error })
    }
  }

  // Array.prototype.group not available in node yet
  // const percentErrorByWeekday = absolutePercentError.group(err => err.date.toLocaleDateString(undefined, {weekday: 'long'}))
  const percentErrorByWeekday = absolutePercentError.reduce((group, {date, error}) => {
    const weekday = date.toLocaleDateString(undefined, {weekday: 'long'})
    group[weekday] = group[weekday] ?? [];
    group[weekday].push(error);
    return group;
  }, {});

  let mapeByWeekday = []
  for (let weekday in percentErrorByWeekday) {
    let allErrsOnThisDay = percentErrorByWeekday[weekday]
    let mape = allErrsOnThisDay.reduce((a, b) => a + b) / allErrsOnThisDay.length 
    mapeByWeekday.push({weekday, mape})
  }

  mapeByWeekday.sort(({mape: mape1}, {mape: mape2}) => mape2 - mape1)
  for (let {weekday, mape} of mapeByWeekday) {
    console.log(`MAPE on ${weekday}s: ${mape * 100.0}%`)
  }
}

getDataViaApi().then(({observations, fcsts}) => createReport(observations, fcsts))
