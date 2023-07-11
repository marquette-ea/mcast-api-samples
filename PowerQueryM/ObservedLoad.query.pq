let
    // These values must be changed to match your data
    MCastDomain = "demo-gas.mea-analytics.tools",
    ApiKey = "your API Key here",
    OpArea = "Metropolis",
    Idf = "Nom Plan",

    // Change these values to alter what data gets included in the analysis 
    MaxDate = DateTime.Date(DateTime.LocalNow()),
    MinDate = Date.AddDays(MaxDate, -60),

    ObservedQueryParams = [
        operatingArea = "Metropolis",
        startDate = Date.ToText(MinDate, "yyyy-MM-dd"),
        endDate = Date.ToText(MaxDate, "yyyy-MM-dd")        
    ],
    ObservedQuery = Uri.BuildQueryString(ObservedQueryParams),
    ObservedUrl = MCastDomain & "/api/v1/daily/observed-load?" & ObservedQuery,
    Headers = [#"x-api-key"=ApiKey],
    Options = [Headers=Headers],
    ObservedLoad = Json.Document(Web.Contents(ObservedUrl, Options))    

in
    Table.FromRecords(ObservedLoad)
