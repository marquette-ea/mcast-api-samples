
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// JSON response for an 8-day load forecast
record ForecastResponseJson (
  string OperatingArea,
  DateOnly ForecastStartDate,
  DateTime UtcRetrievalTimestamp,
  DateTime UtcForecastTimestamp,
  bool IsPinned,
  string Idf,
  List<ForecastData> LoadForecast
);

// JSON response for a single load forecast value
record ForecastData (
  DateOnly Date,
  int DaysOut,
  double Forecast
);

// JSON response for an observed load value
record ObservedResponseJson (
  string OperatingArea,
  DateOnly Date,
  double NetLoad,
  DateTime UtcRetrievalTimestamp
);

record ReportData (
  List<ObservedResponseJson> Observations,
  List<ForecastResponseJson> Forecasts
);

class CreateMapeByWeekdayReport {
  // This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
  // You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
  static readonly string ApiKey = System.Environment.GetEnvironmentVariable("MCAST_API_KEY")!;

  // Change this to your company's unique MCast domain
  static readonly string MCastDomain = "demo-gas.mea-analytics.tools";

  static readonly string OpArea = "Metropolis";
  static readonly DateOnly MaxDate = DateOnly.FromDateTime(DateTime.Today);
  static readonly DateOnly MinDate = MaxDate.AddDays(-60);
  static readonly string Idf = "Nom Plan";

  static readonly HttpClient Client = new HttpClient();

  public static async Task Run() { 
    var data = await GetDataViaAPI();
    CreateReport(data);
  }

  // This retrieves the raw data from the MCast API that we will need in order to compute the MAPE by weekday
  static async Task<ReportData> GetDataViaAPI() {
    Client.DefaultRequestHeaders.Accept.Clear();
    Client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

    Console.WriteLine("Retrieving forecast data...");

    var query = new Dictionary<string, string> {  
      ["operatingArea"] = OpArea,
      ["startDate"] = MinDate.ToShortDateString(),
      ["endDate"] = MaxDate.ToShortDateString(),
      ["idf"] = Idf,
    };
    var uri = QueryHelpers.AddQueryString($"https://{MCastDomain}/api/v1/daily/forecasted-load", query);
    var response = await Client.GetAsync(uri);
    Console.WriteLine(response.ToString());
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var fcsts = JsonConvert.DeserializeObject<List<ForecastResponseJson>>(json)!;

    Console.WriteLine("Retrieving observed data...");

    query = new Dictionary<string, string> {  
      ["operatingArea"] = OpArea,
      ["startDate"] = MinDate.ToShortDateString(),
      ["endDate"] = MaxDate.ToShortDateString(),
    };
    uri = QueryHelpers.AddQueryString($"https://{MCastDomain}/api/v1/daily/observed-load", query);
    response = await Client.GetAsync(uri);
    Console.WriteLine(response.ToString());
    response.EnsureSuccessStatusCode();
    json = await response.Content.ReadAsStringAsync();
    var observations = JsonConvert.DeserializeObject<List<ObservedResponseJson>>(json)!;

    return new ReportData(observations, fcsts);
  }

  // This takes the data we've already retrieved from the MCast API and computes the MAPE score for each weekday,
  // printing the results out to the Console.
  static void CreateReport(ReportData data) {

    // "horizon" means how many days ahead the forecast is looking.  Each MCast forecast has horizons 0 through 7, meaning
    // it forecasts "today" through "next week" (7 days from "today").  This report will examine the MAPE of forecasts made
    // for horizon 1, so looking at how accurate the forecasts are for "tomorrow".
    var forecastHorizon = 1;

    var tomorrowForecasts = from fcst in data.Forecasts select fcst.LoadForecast.Find(f => f.DaysOut == forecastHorizon);
    var fcstsByDate = tomorrowForecasts.ToDictionary(
      keySelector: fcst => fcst.Date,
      elementSelector: fcst => fcst.Forecast
    );

    var observationsByDate = data.Observations.ToDictionary(
      keySelector: obs => obs.Date,
      elementSelector: obs => obs.NetLoad
    );

    var dateRange = new List<DateOnly>();
    for (var date = MinDate; date <= MaxDate; date = date.AddDays(1)) { dateRange.Add(date); }

    var absolutePercentError = 
      from date in dateRange 
      where observationsByDate.ContainsKey(date) && fcstsByDate.ContainsKey(date)
      let percentError = Math.Abs((fcstsByDate[date] - observationsByDate[date]) / observationsByDate[date])
      select new { Date = date, Error = percentError };

    var mapeByWeekday = 
      from err in absolutePercentError
      group err by err.Date.DayOfWeek into weekdayErr
      let weekday = weekdayErr.Key
      let mape = (from x in weekdayErr select x.Error).Average()
      select new { Weekday = weekday, MAPE = mape };

    foreach (var err in mapeByWeekday.OrderByDescending(x => x.MAPE)) {
      Console.WriteLine($"MAPE on {err.Weekday.ToString()}s: {err.MAPE * 100.0}%");
    }
  }
}
