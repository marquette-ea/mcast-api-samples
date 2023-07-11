let
    MCastDomain = "demo-gas.mea-analytics.tools",
    MaxDate = DateTime.Date(DateTime.LocalNow()),
    MinDate = Date.AddDays(MaxDate, -60),
    ApiKey = "your API Key here",
    Headers = [#"x-api-key"=ApiKey],
    Options = [Headers=Headers],

    ForecastHorizon = 1,
    ForecastQueryParams = [ 
        operatingArea = "Metropolis",
        startDate = Date.ToText(MinDate, "yyyy-MM-dd"),
        endDate = Date.ToText(MaxDate, "yyyy-MM-dd"),
        idf = "Nom Plan"
    ],
    ForecastQuery = Uri.BuildQueryString(ForecastQueryParams),
    ForecastUrl = MCastDomain & "/api/v1/daily/forecasted-load?" & ForecastQuery,
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
