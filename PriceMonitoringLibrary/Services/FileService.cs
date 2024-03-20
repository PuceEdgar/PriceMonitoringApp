using Newtonsoft.Json;
using PriceMonitoringLibrary.Models;
using System.Collections.ObjectModel;

namespace PriceMonitoringLibrary.Services;

public static class FileService
{
    private static readonly string _appData = FileSystem.AppDataDirectory;
    private static readonly string _path = Path.Combine(_appData, Constants.FileName);

    public static async Task<bool> SaveToFile(ObservableCollection<MonitoredItem> items)
    {
        if (items == null) return false;
        var stringItems = JsonConvert.SerializeObject(items);
        await File.WriteAllTextAsync(_path, stringItems);
        return true;
    }

    public static async Task<List<MonitoredItem>> GetSavedItemData()
    {
        var data = await GetDataFromDataFile();

        return string.IsNullOrWhiteSpace(data) ? [] : JsonConvert.DeserializeObject<List<MonitoredItem>>(data)!;
    }

    private static async Task<string> GetDataFromDataFile()
    {
        if (!File.Exists(_path))
        {
            File.Create(_path).Close();
            return string.Empty;
        }
        else
        {
            return await File.ReadAllTextAsync(_path);
        }
    }
}
