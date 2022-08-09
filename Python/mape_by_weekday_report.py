
import os
import requests
import dateutil.parser
import calendar
from datetime import date, datetime, timedelta
from dataclasses import dataclass
from statistics import mean
from itertools import groupby

# This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
# You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
api_key = os.environ["MCAST_API_KEY"]

# Change this to your company's unique MCast domain
mcast_domain = "demo-gas.mea-analytics.tools"

max_date = date.today()
min_date = max_date + timedelta(days=-60)
op_area = "Metropolis"
idf = "1"

# "horizon" means how many days ahead the forecast is looking.  Each MCast forecast has horizons 0 through 7, meaning
# it forecasts "today" through "next week" (7 days from "today").  This report will examine the MAPE of forecasts made
# for horizon 1, so looking at how accurate the forecasts are for "tomorrow".
forecast_horizon = 1


@dataclass(frozen=True)
class ForecastData:
  """JSON response for a single load forecast value"""
  date: date
  days_out: int
  forecast: float

  @classmethod
  def of_dict(cls, d):
    """Parse a dictionary compatible with JSON into an ForecastData"""
    return ForecastData(
      date=date.fromisoformat(d["date"]),
      days_out=int(d["daysOut"]),
      forecast=float(d["forecast"]),
    )

@dataclass(frozen=True)
class ForecastResponseJson:
  """JSON response for an 8-day load forecast"""
  operating_area: str
  forecast_start_date: date
  utc_retrieval_timestamp: datetime
  utc_forecast_timestamp: datetime
  is_pinned: bool
  idf: int
  load_forecast: list[ForecastData]

  @classmethod
  def of_dict(cls, d):
    """Parse a dictionary compatible with JSON into an ForecastResponseJson"""
    return ForecastResponseJson(
      operating_area=d["operatingArea"],
      forecast_start_date=date.fromisoformat(d["forecastStartDate"]),
      utc_retrieval_timestamp=dateutil.parser.isoparse(d["utcRetrievalTimestamp"]),
      utc_forecast_timestamp=dateutil.parser.isoparse(d["utcRetrievalTimestamp"]),
      is_pinned=bool(d["isPinned"]),
      idf=int(d["idf"]),
      load_forecast=[ ForecastData.of_dict(fcst) for fcst in d["loadForecast"] ],
    )

@dataclass(frozen=True)
class ObservedResponseJson:
  """JSON response for an observed load value"""
  operating_area: str
  date: date
  net_load: float
  utc_retrieval_timestamp: datetime

  @classmethod
  def of_dict(cls, d):
    """Parse a dictionary compatible with JSON into an ObservedResponseJson"""
    return ObservedResponseJson(
      operating_area=d["operatingArea"],
      date=date.fromisoformat(d["date"]),
      net_load=float(d["netLoad"]),
      utc_retrieval_timestamp=dateutil.parser.isoparse(d["utcRetrievalTimestamp"]),
    )


def get_data_via_api():
  """ This retrieves the raw data from the MCast API that we will need in order to compute the MAPE by weekday """

  print("Retrieving forecast data...")

  params = {  
    "operatingArea": op_area,
    "startDate": min_date.isoformat(),
    "endDate": max_date.isoformat(),
    "idf": idf,
  }

  headers = { "x-api-key": api_key }

  response = requests.get(f"https://{mcast_domain}/api/v1/daily/forecasted-load", params=params, headers=headers)
  print(response)
  response.raise_for_status()
  fcsts = [ ForecastResponseJson.of_dict(json) for json in response.json() ]

  print("Retrieving observed data...")

  params = {  
    "operatingArea": op_area,
    "startDate": min_date.isoformat(),
    "endDate": max_date.isoformat(),
  }

  headers = { "x-api-key": api_key }

  response = requests.get(f"https://{mcast_domain}/api/v1/daily/observed-load", params=params, headers=headers);
  print(response)
  response.raise_for_status()
  observations = [ ObservedResponseJson.of_dict(json) for json in response.json() ]

  return (observations, fcsts)


def create_report(observations, fcsts):
  """
  This takes the data we've already retrieved from the MCast API and computes the MAPE score for each weekday,
  printing the results out to the console.
  """

  tomorrow_forecasts = [
    fcst_day 
    for fcst in fcsts 
    for fcst_day in fcst.load_forecast 
    if fcst_day.days_out == forecast_horizon
  ]

  fcsts_by_date = {
    fcst.date: fcst.forecast
    for fcst in tomorrow_forecasts
  }

  observations_by_date = {
    obs.date: obs.net_load
    for obs in observations
  }

  dates = []
  date = min_date
  while date <= max_date:
    dates.append(date)
    date = date + timedelta(days=1)

  absolute_percent_error = [
    { "date": date, "error": (fcsts_by_date[date] - observations_by_date[date]) / observations_by_date[date] }
    for date in dates
    if date in observations_by_date and date in fcsts_by_date
  ]

  def err_weekday(err): 
    return err["date"].weekday()

  percent_error_by_weekday = groupby(sorted(absolute_percent_error, key=err_weekday), key=err_weekday)
  mape_by_weekday = [
    (weekday, mean(x["error"] for x in err))
    for weekday, err in percent_error_by_weekday
  ]

  for (weekday, mape) in sorted(mape_by_weekday, key=lambda err: err[1], reverse=True):
    print(f"MAPE on {calendar.day_name[weekday]}s: {mape * 100.0}%");



(observations, fcsts) = get_data_via_api()
create_report(observations, fcsts)
