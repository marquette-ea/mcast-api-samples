
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// Data format the API expects in the request to upload data
record RequestJson ( 
  string OperatingArea,
  DateOnly Date,
  double Load
);

class UploadData {
  // This expects to find an environment variable named MCAST_API_KEY containing the key obtained from the MCast web interface.
  // You may alternatively paste the API key on this line, though your company's policy might discourage plain-text secrets.
  static readonly string ApiKey = System.Environment.GetEnvironmentVariable("MCAST_API_KEY")!;

  // Change this to your company's unique MCast domain
  static readonly string MCastDomain = "demo-gas.mea-analytics.tools";

  // This will just test the API call without making any changes.
  // Change this to "false" to make real changes to your MCast database.
  static readonly bool DryRun = true;

  static readonly HttpClient Client = new HttpClient();

  // This lets us use PascalCase for our field names in the RequestJson record defined above, which is standard for C#
  // even though the JSON we receive from the API uses camelCase.
  static readonly JsonSerializerSettings SerializerSettings = 
    new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
  
  public static async Task Run() {
    Client.DefaultRequestHeaders.Accept.Clear();
    Client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

    var dates = (from day in Enumerable.Range(25, 7) select new DateOnly(2022,07,day)).ToList();

    var metropolisLoad = new double[] { 134537, 135647, 136389, 137446, 132354, 124888, 127618 };
    var smallvilleLoad = new double[] { 33409, 33033, 32765, 34437, 33164, 31412, 30426 };
    var body = new List<RequestJson>();

    for (var i = 0; i <= 6; i++) {

      body.Add(
        new RequestJson(
          OperatingArea: "Metropolis",
          Date: dates[i],
          Load: metropolisLoad[i]
        )
      );

      body.Add(
        new RequestJson(
          OperatingArea: "Smallville",
          Date: dates[i],
          Load: smallvilleLoad[i]
        )
      );
    }
        
    var query = new Dictionary<string, string> { ["dryRun"] = DryRun.ToString() };
    var uri = QueryHelpers.AddQueryString($"https://{MCastDomain}/api/v1/daily/observed-load", query);
    var content = new StringContent(
      JsonConvert.SerializeObject(body, SerializerSettings), 
      encoding: Encoding.UTF8, 
      mediaType: "application/json"
    );

    Console.WriteLine("submitting load values...");
    var response = await Client.PostAsync(uri, content);
    Console.WriteLine(response.ToString());
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    response.EnsureSuccessStatusCode();

    Console.WriteLine("");
    Console.WriteLine("Generating a forecast...");

    uri = QueryHelpers.AddQueryString($"https://{MCastDomain}/api/v1/daily/generate-forecast", query);
    response = await Client.PostAsync(uri, null);
    Console.WriteLine(response.ToString());
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    response.EnsureSuccessStatusCode();
  }
}