This folder has two samples of using the MCast™ API.  You can run `node upload-raw-data.js` or `node create-mape-by-weekday-report.js` to run either example. Note that you'll need a recent version of Node.js installed in order to run these examples.

The first sample, upload-raw-data.js, demonstrates using the MCast™ API to upload new consumption data and run a new forecast. Note that running this sample as-is won't actually make any changes on your MCast™ system, since the `dryRun` flag is set to `true`.

The second sample, create-mape-by-weekday-report.js, demonstrates using the MCast™ API to download recent consumption data and MCast™ forecasts to run a custom report. In this example, we compute recent accuracy (using MAPE) by weekday and print the results.