let
    // These values must be changed to match your data
    MCastDomain = "demo-gas.mea-analytics.tools",
    ApiKey = "your API Key here",
    OpArea = "Metropolis",
    Idf = "Nom Plan",

    // Change these values to alter what data gets included in the analysis 
    MaxDate = DateTime.Date(DateTime.LocalNow()),
    MinDate = Date.AddDays(MaxDate, -60),
    ForecastHorizon = 1,

    ForecastQueryParams = [ 
        operatingArea = OpArea,
        startDate = Date.ToText(MinDate, "yyyy-MM-dd"),
        endDate = Date.ToText(MaxDate, "yyyy-MM-dd"),
        idf = Idf
    ],
    ForecastQuery = Uri.BuildQueryString(ForecastQueryParams),
    ForecastUrl = MCastDomain & "/api/v1/daily/forecasted-load?" & ForecastQuery,
    Headers = [#"x-api-key"=ApiKey],
    Options = [Headers=Headers],
    ForecastedLoad = Json.Document(Web.Contents(ForecastUrl, Options)),
    Flattened = List.TransformMany(
        ForecastedLoad, 
        each [loadForecast],
        (fcst, results) => [
            operatingArea = fcst[operatingArea],
            fcstForDate = results[date],
            daysOut = results[daysOut],
            value = results[forecast]
        ]
    ),
    Day1Forecasts = List.Select(Flattened, each [daysOut] = ForecastHorizon)

in
    Table.FromRecords(Day1Forecasts)
