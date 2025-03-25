NOTE: This folder is for Windows PowerShell (version 5.x or below). See the PowerShell folder for newer versions of PowerShell.

This folder has two samples of using the MCast™ API. The first sample, Send-RawData.ps1, demonstrates using the MCast™ API to upload new consumption data directly and then run a new forecast. The second sample, Send-DataFile.ps1, demonstrates using the MCast™ API to upload a file (formatted according to what MCast™ expects as worked out with your MEA support contact) to the MCast™ servers, which will automatically trigger a forecast. The second sample could be run on your own MCast™ instance by simply swapping in your own data file.

Note that running either of these samples as-is won't actually make any changes on your MCast™ system, since the `dryRun` flag is set to `true`.
