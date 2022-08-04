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

  static async Task Main(string[] args) {

    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

    await UploadData();
  }
}