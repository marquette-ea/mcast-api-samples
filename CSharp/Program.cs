
class Program { 
  static async Task Main(string[] args) {
    await UploadData.Run();
    await CreateMapeByWeekdayReport.Run();
  }
}