let
    MCastDomain = "demo-gas.mea-analytics.tools",
    MaxDate = DateTime.Date(DateTime.LocalNow()),
    MinDate = Date.AddDays(MaxDate, -60),
    ApiKey = "your API Key here"
    Headers = [#"x-api-key"=ApiKey],
    Options = [Headers=Headers],


    ObservedQueryParams = [
        operatingArea = "Metropolis",
        startDate = Date.ToText(MinDate, "yyyy-MM-dd"),
        endDate = Date.ToText(MaxDate, "yyyy-MM-dd")        
    ],
    ObservedQuery = Uri.BuildQueryString(ObservedQueryParams),
    ObservedUrl = MCastDomain & "/api/v1/daily/observed-load?" & ObservedQuery,
    ObservedLoad = Json.Document(Web.Contents(ObservedUrl, Options))    

in
    Table.FromRecords(ObservedLoad)
