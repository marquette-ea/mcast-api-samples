using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;

record PostJson ( 
  string OperatingArea,
  DateOnly Date,
  double Load
);

record GetForecastJson (
  string OperatingArea,
  DateOnly ForecastStartDate,
  DateTime UtcRetrievalTimestamp,
  DateTime UtcForecastTimestamp,
  bool IsPinned,
  int Idf,
  List<ForecastData> LoadForecast
);

record ForecastData (
  DateOnly Date,
  int DaysOut,
  double Forecast
);

record GetObservedJson (
  string OperatingArea,
  DateOnly Date,
  double NetLoad,
  DateTime UtcRetrievalTimestamp
);


class Program { 
  // This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
  // You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
  static readonly string apiKey = System.Environment.GetEnvironmentVariable("MCAST_API_KEY")!;

  // Change this to your company's unique MCast domain
  static readonly string mcastDomain = "demo-gas.mea-analytics.tools";
  static readonly HttpClient client = new HttpClient();
  static readonly JsonSerializerSettings serializerSettings = 
    new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
  
  static async Task UploadData() {
    // This will just test the API call without making any changes.
    // Change this to "false" to make real changes to your MCast database.
    var dryRun = true;

    var dates = (from day in Enumerable.Range(25, 7) select new DateOnly(2022,07,day)).ToList();

    var metropolisLoad = new double[] { 134537, 135647, 136389, 137446, 132354, 124888, 127618 };
    var smallvilleLoad = new double[] { 33409, 33033, 32765, 34437, 33164, 31412, 30426 };
    var body = new List<PostJson>();

    for (var i = 0; i <= 6; i++) {

      body.Add(
        new PostJson(
          OperatingArea: "Metropolis",
          Date: dates[i],
          Load: metropolisLoad[i]
        )
      );

      body.Add(
        new PostJson(
          OperatingArea: "Smallville",
          Date: dates[i],
          Load: smallvilleLoad[i]
        )
      );
    }
        
    var query = new Dictionary<string, string> { ["dryRun"] = dryRun.ToString() };
    var uri = QueryHelpers.AddQueryString($"https://{mcastDomain}/api/v1/daily/observed-load", query);
    var content = new StringContent(
      JsonConvert.SerializeObject(body, serializerSettings), 
      encoding: Encoding.UTF8, 
      mediaType: "application/json"
    );

    Console.WriteLine("submitting load values...");
    var response = await client.PostAsync(uri, content);
    Console.WriteLine(response.ToString());
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    response.EnsureSuccessStatusCode();

    Console.WriteLine("");
    Console.WriteLine("Generating a forecast...");

    uri = QueryHelpers.AddQueryString($"https://{mcastDomain}/api/v1/daily/generate-forecast", query);
    response = await client.PostAsync(uri, null);
    Console.WriteLine(response.ToString());
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    response.EnsureSuccessStatusCode();
  }

  static async Task CreateMapeByWeekdayReport() { 
    var maxDate = DateOnly.FromDateTime(DateTime.Today);
    var minDate = maxDate.AddDays(-60);
    var opArea = "Metropolis";
    
    // "horizon" means how many days ahead the forecast is looking.  Each MCast forecast has horizons 0 through 7, meaning
    // it forecasts "today" through "next week" (7 days from "today").  This report will examine the MAPE of forecasts made
    // for horizon 1, so looking at how accurate the forecasts are for "tomorrow".
    var forecastHorizon = 1;


    Console.WriteLine("Retrieving forecast data...");

    var query = new Dictionary<string, string> {  
      ["operatingArea"] = opArea,
      ["startDate"] = minDate.ToShortDateString(),
      ["endDate"] = maxDate.ToShortDateString(),
      ["idf"] = "1",
    };
    var uri = QueryHelpers.AddQueryString($"https://{mcastDomain}/api/v1/daily/forecasted-load", query);
    var response = await client.GetAsync(uri);
    Console.WriteLine(response.ToString());
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var fcsts = JsonConvert.DeserializeObject<List<GetForecastJson>>(json)!;

    Console.WriteLine("Retrieving observed data...");

    query = new Dictionary<string, string> {  
      ["operatingArea"] = opArea,
      ["startDate"] = minDate.ToShortDateString(),
      ["endDate"] = maxDate.ToShortDateString(),
    };
    uri = QueryHelpers.AddQueryString($"https://{mcastDomain}/api/v1/daily/observed-load", query);
    response = await client.GetAsync(uri);
    Console.WriteLine(response.ToString());
    response.EnsureSuccessStatusCode();
    json = await response.Content.ReadAsStringAsync();
    var observations = JsonConvert.DeserializeObject<List<GetObservedJson>>(json)!;

    var tomorrowForecasts = from fcst in fcsts select fcst.LoadForecast.Find(f => f.DaysOut == forecastHorizon);
    var fcstsByDate = tomorrowForecasts.ToDictionary(
      keySelector: fcst => fcst.Date,
      elementSelector: fcst => fcst.Forecast
    );

    var observationsByDate = observations.ToDictionary(
      keySelector: obs => obs.Date,
      elementSelector: obs => obs.NetLoad
    );

    var dates = new List<DateOnly>();
    for (var date = minDate; date <= maxDate; date = date.AddDays(1)) { dates.Add(date); }

    var absolutePercentError = 
      from date in dates 
      where observationsByDate.ContainsKey(date) && fcstsByDate.ContainsKey(date)
      let percentError = (fcstsByDate[date] - observationsByDate[date]) / observationsByDate[date]
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

  static async Task Main(string[] args) {

    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

    await UploadData();
    await CreateMapeByWeekdayReport();
  }
}