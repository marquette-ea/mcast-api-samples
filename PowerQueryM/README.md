This folder has three sample Power Query M queries that can be used to load MCast™ data into Excel or PowerBI.

The first sample, ObservedLoad.pq, uses the MCast™ API to populate a table with the last 60 days of consumption data. The second sample, ForecastedLoad.pq, uses the MCast™ API to populate a table with all of the pinned one-day-ahead MCast™ forecast values from the last 60 days. The third sample, APE.pq, merges the results of the previous two queries and adds columns for the day of week and the absolute percent error.

To run these samples, open Excel and in the "Data" tab, select "Get Data" -> "From Other Sources" -> "Blank Query" to create a new query. 

![BlankQuery]

In your new query, under "Query," select "Advanced Editor" and paste in the code from any of the three samples. For ObservedLoad.pq or ForecastedLoad.pq, you'll have to edit the query to use your own domain and API Key. For APE.pq, you'll have to setup the other two examples first.

![AdvEditor]

Finally, select "Close & Load" to finish setting up the query. By default, any connection will load in a tab in a new table. 

[BlankQuery]: ../assets/BlankQuery.png "Create Blank Query"
[AdvEditor]: ../assets/AdvancedEditor.png "Advanced Editor"
