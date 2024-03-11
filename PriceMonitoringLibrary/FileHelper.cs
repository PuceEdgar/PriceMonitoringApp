using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace PriceMonitoringLibrary;

public static class FileHelper
{
    private static readonly string _file = "ItemData.json";
    private static readonly string _appData = FileSystem.AppDataDirectory;
    private static readonly string _path = Path.Combine(_appData, _file);

    public static async Task<bool> SaveToFile(ObservableCollection<MonitoredItem> items)
    {
        if (items == null) return false;        
        var stringItems = JsonConvert.SerializeObject(items);
        await File.WriteAllTextAsync(_path, stringItems);
        return true;
    }

    public static async Task<List<MonitoredItem>> GetSavedItemData()
    {
        var data = await File.ReadAllTextAsync(_path);
        List<MonitoredItem>? items = JsonConvert.DeserializeObject<List<MonitoredItem>>(data);
        if (items is null)
        {
            return [];
        }

        return items;
    }
}
