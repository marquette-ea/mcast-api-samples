let
    Source = Table.NestedJoin(ForecastedLoad, {"fcstForDate", "operatingArea"}, ObservedLoad, {"date", "operatingArea"}, "ObservedLoad", JoinKind.Inner),
    Expanded = Table.ExpandTableColumn(Source, "ObservedLoad", {"netLoad"}, {"ObservedLoad"}),
    Renamed = Table.RenameColumns(Expanded,{{"value", "ForecastedLoad"}}),
    AddedWeekday = Table.AddColumn(Renamed, "Weekday", each Date.DayOfWeekName([fcstForDate])),
    AddedAPE = Table.AddColumn(AddedWeekday, "APE", each Number.Abs(([ForecastedLoad] - [ObservedLoad]) / [ObservedLoad]) * 100.0)
in
    AddedAPE
