This project has two samples of using the MCast™ API in the same C# project.  From this folder, you can run `dotnet run` to install all dependencies, build the project, and run both samples sequentially.  If you want to only run a single sample, just comment out one of the sample runs in Program.cs. Note that you'll need the .net SDK for .net 6.0 installed in order to run these examples.

The first sample, UploadData.cs, demonstrates using the MCast™ API to upload new consumption data and run a new forecast. Note that running this sample as-is won't actually make any changes on your MCast™ system, since the `dryRun` flag is set to `true`.

The second sample, CreateMapeByWeekdayReport.cs, demonstrates using the MCast™ API to download recent consumption data and MCast™ forecasts to run a custom report. In this example, we compute recent accuracy (using MAPE) by weekday and print the results.
