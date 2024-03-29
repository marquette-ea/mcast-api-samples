This folder has three sample Power Query M queries that can be used to load MCast™ data into Excel or PowerBI.

The first sample, ObservedLoad.query.pq, uses the MCast™ API to populate a table with the last 60 days of consumption data. The second sample, ForecastedLoad.query.pq, uses the MCast™ API to populate a table with all of the pinned one-day-ahead MCast™ forecast values from the last 60 days. The third sample, APE.query.pq, merges the results of the previous two queries and adds columns for the day of week and the absolute percent error.

To run these samples, open Excel and in the "Data" tab, select "Get Data" -> "From Other Sources" -> "Blank Query" to create a new query. 

![BlankQuery]

In your new query, under "Query," select "Advanced Editor" to open a code window to paste in the code from any of the three samples. 

![AdvEditorButton]
![AdvEditor]

For ObservedLoad.query.pq or ForecastedLoad.query.pq, you'll have to edit the query after pasting to use your own domain and API Key. For APE.query.pq, you'll have to setup the other two examples first.

Finally, select "Close & Load" to finish setting up the query. By default, any connection will load in a tab in a new table. 

[BlankQuery]: ../assets/BlankQuery.png "Create Blank Query"
[AdvEditorButton]: ../assets/AdvancedEditorButton.png "Button to Open Advanced Editor"
[AdvEditor]: ../assets/AdvancedEditor.png "Advanced Editor"
