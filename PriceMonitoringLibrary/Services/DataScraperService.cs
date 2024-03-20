using HtmlAgilityPack;
using PriceMonitoringApp;
using PriceMonitoringLibrary.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PriceMonitoringLibrary.Services;

public static class DataScraperService
{
    public static async Task<MonitoredItem?> GetItemFromUrl(string url)
    {        
        var nodes = await GetNodesFromProvidedUrl(url);
        if (nodes is null || nodes.Count() < 700)
        {
            return null;
        }

        return CollectItemInfoFromHanstyleMobile(nodes, url);
    }

    public static async Task<bool> CheckIfItemDetailsHaveChanged()
    {
        var detailsChanged = false;
        var monitoredItems = await FileService.GetSavedItemData();

        foreach (var item in monitoredItems)
        {
            var newItemData = await GetItemFromUrl(item.ShareUrl!);
            if (item.Price != newItemData?.Price)
            {
                detailsChanged = true;
                item.PreviousPrice = item.Price;
                item.PriceHistory.Add(new HistoryDetails { Price = item.Price });
                item.Price = newItemData?.Price;
            }
            var result = item.AvailableSizes?.Except(newItemData?.AvailableSizes, new SizeDetailsComparer()).ToList();
            if (result?.Count > 0)
            {
                detailsChanged = true;
                item.AvailableSizes = newItemData?.AvailableSizes;
            }
        }

        if (detailsChanged)
        {
            await FileService.SaveToFile(new ObservableCollection<MonitoredItem>(monitoredItems));
        }

        return detailsChanged;
    }

    private static MonitoredItem CollectItemInfoFromHanstyleMobile(IEnumerable<HtmlNode> nodes, string url)
    {
        string? brand = GetValueForProperty(nodes, Constants.BrandProperty);
        var salePrice = GetValueForProperty(nodes, Constants.SalePriceProperty);
        var title = GetValueForProperty(nodes, Constants.TitleProperty);
        var img = GetValueForProperty(nodes, Constants.ImageProperty);
        var price = GetValueForProperty(nodes, Constants.PriceProperty);
        var productUrl = GetValueForProperty(nodes, Constants.UrlProperty);
        var options = nodes?.FirstOrDefault(n => n.HasClass(Constants.OptionSelectBoxTag))?.ChildNodes?.FirstOrDefault(n => n.Name == "ul")?.ChildNodes.Where(n => n.Name == "li");
        var allSizes = new List<SizeDetails>();
        var availableSizes = new List<SizeDetails>();

        foreach (var option in options)
        {
            var size = option.ChildNodes[1]?.InnerText;
            var availability = option.ChildNodes.Count > 4 ? option.ChildNodes[3]?.InnerText.Trim() : "Many";
            allSizes.Add(new SizeDetails(size, availability));
            if (!availability.Contains(Constants.SoldOut))
            {
                availableSizes.Add(new SizeDetails(size, availability));
            }
        }

        var isSoldOut = IsItemSoldOut(nodes!, allSizes);
        var mobilepriceNodeChildren = nodes!.FirstOrDefault(n => n.HasClass("price"))?.ChildNodes;
        var discountPercent = mobilepriceNodeChildren?.FirstOrDefault(c => c.Name == "span")?.InnerText.Trim();

        MonitoredItem item = new()
        {
            Brand = brand,
            Description = title,
            Price = FormatStringToCurrency(salePrice),
            InitialProductPrice = FormatStringToCurrency(price),
            OriginalPrice = FormatStringToCurrency(price),
            DiscountPercent = discountPercent,
            AllSizes = allSizes,
            AvailableSizes = availableSizes,
            ImageUrl = $"https:{img}",
            IsSoldOut = isSoldOut,
            ProductUrl = productUrl,
            AvailableSizesAsString = $"\n{string.Join("  ", availableSizes?.Select(s => $"[ {s.Size} ]"))}",
            ShareUrl = url,
            ProductCode = UriService.GetProductCodeValueFromUri(url)
        };

        return item;
    }

    private static string? GetValueForProperty(IEnumerable<HtmlNode> nodes, string propertyName)
    {
        return nodes?.FirstOrDefault(n => n.Name == Constants.Meta && n.Attributes.AttributesWithName(Constants.Property).Any(a => a.Value.Equals(propertyName)))?.Attributes[1].Value;
    }

    private static string FormatStringToCurrency(string price)
    {
        return $"￦{string.Format(CultureInfo.InvariantCulture, "{0:n0}", int.Parse(price))}";
    }

    private static bool IsItemSoldOut(IEnumerable<HtmlNode> nodes, List<SizeDetails> sizes)
    {
        var soldOutMetaExists = nodes.Any(n => n.Name == Constants.Meta && n.Attributes.AttributesWithName(Constants.Property).Any(a => a.Value.Equals(Constants.AvailabilityProperty)));
        var noSizeLeft = sizes!.TrueForAll(s => s.Availability.Trim().Contains(Constants.SoldOut));
        return noSizeLeft || soldOutMetaExists;
    }

    private static async Task<IEnumerable<HtmlNode>> GetNodesFromProvidedUrl(string url)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
        using HttpResponseMessage response = await client.GetAsync(url);
        using HttpContent content = response.Content;

        string result = await content.ReadAsStringAsync();
        HtmlDocument doc = new();
        doc.LoadHtml(result);

        return doc.DocumentNode.Descendants(0);
    }
}
