using HtmlAgilityPack;
using PriceMonitoringApp;
using PriceMonitoringLibrary.Enums;
using PriceMonitoringLibrary.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json.Nodes;

namespace PriceMonitoringLibrary.Services;

public static class DataScraperService
{
    public static async Task<MonitoredItem?> GetItemFromUrl(string url)
    {
        var content = await GetStringContentFromProvidedUrl(url);

        var nodes = GetNodesFromContent(content);

        if (nodes is null || nodes.Count() < 400)
        {
            return null;
        }

        var item = url switch
        {
            string u when u.Contains(Constants.MobileHanstyleDomainName) => CollectItemInfoFromHanstyleMobile(nodes, url),
            string u when u.Contains(Constants.MusinsaDomainName) => await CollectItemInfoFromMusinsaMobile(content, url),
            _ => null
        };

        return item;
    }

    public static async Task<bool> CheckIfItemDetailsHaveChanged()
    {
        var detailsChanged = false;
        var monitoredItems = await FileService.GetSavedItemData();

        foreach (var item in monitoredItems)
        {
            var newItemData = item.ShopName == ShopName.Hanstyle ? await GetItemFromUrl(item.ProductUrl!) : await CollectMusinsaDetailsForId(item.Id!);

            if (newItemData is null)
            {
                await NotificationService.ShowGeneralErrorNotification(item);
                return detailsChanged;
            }

            if (!string.IsNullOrWhiteSpace(newItemData.Price) && item.Price != newItemData.Price)
            {
                item.IsPriceCheaper = IsPriceCheaper(item.Price!, newItemData.Price);
                detailsChanged = true;
                item.PreviousPrice = item.Price;
                item.PriceHistory.Add(new HistoryDetails { Price = item.Price });
                item.Price = newItemData.Price;
            }

            var result = item.AvailableSizes?.Except(newItemData.AvailableSizes!, new SizeDetailsComparer()).ToList();

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

    public static async Task<MonitoredItem> CollectMusinsaDetailsForId(string id, string? productUrl = null)
    {
        var item = new MonitoredItem
        {
            Id = id,
            ShopName = ShopName.Musinsa
        };
        var itemDetailsJson = await GetStringContentFromProvidedUrl(Constants.GetMusinsaItemDetailUrl(id));
        PopulateItemDetails(item, itemDetailsJson, productUrl);

        var remainingSizeJson = await GetStringContentFromProvidedUrl(Constants.GetMusinsaRemainingSizeUrl(id));
        PopulateSizeDetails(item, remainingSizeJson);
        return item;
    }

    private static async Task<MonitoredItem?> CollectItemInfoFromMusinsaMobile(string content, string productUrl)
    {
        var id = content.Split("goods")[1].Split("?")[0].Split("/")[1];

        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }
        return await CollectMusinsaDetailsForId(id, productUrl);
    }

    private static void PopulateSizeDetails(MonitoredItem item, string remainingSizeJson)
    {
        var dataNode = JsonNode.Parse(remainingSizeJson)!["data"];
        var sizeArray = (JsonArray)dataNode!["basic"]!;

        foreach (var sizeInfo in sizeArray)
        {
            var size = sizeInfo!["name"]!.ToString();
            var availability = sizeInfo!["remainQuantity"]!.ToString();
            item.AllSizes!.Add(new SizeDetails(size, availability));
            if (availability != "0")
            {
                item.AvailableSizes!.Add(new SizeDetails(size, availability));
            }
        }
        item.AvailableSizesAsString = $"\n{string.Join("  ", item.AvailableSizes!.Select(s => $"[ {s.Size} ]"))}";
    }

    private static void PopulateItemDetails(MonitoredItem item, string itemDetailsJson, string? productUrl)
    {
        var dataNode = JsonNode.Parse(itemDetailsJson)!["data"];
        item.Brand = dataNode!["brandNmEng"]!.ToString();
        item.Description = dataNode["goodsNmEng"]!.ToString();
        item.Price = FormatStringToCurrency(dataNode!["goodsPrice"]!["memberPrice"]!.ToString());
        item.OriginalPrice = FormatStringToCurrency(dataNode!["goodsPrice"]!["originPrice"]!.ToString());
        item.InitialProductPrice = item.OriginalPrice;
        item.DiscountPercent = $"{dataNode!["goodsPrice"]!["discountRate"]}%";
        item.ProductUrl = Constants.GetMusinsaItemDetailUrl(item.Id!);
        item.SizeDetailsUrl = Constants.GetMusinsaRemainingSizeUrl(item.Id!);
        item.ImageUrl = Constants.GetMusinsaImageUrl(dataNode["thumbnailImageUrl"]!.ToString());
        item.LinkToProduct ??= productUrl;
    }

    private static IEnumerable<HtmlNode> GetNodesFromContent(string content)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(content);

        return doc.DocumentNode.Descendants(0);
    }

    private static CheaperPrice IsPriceCheaper(string currentPrice, string newPrice)
    {
        var currentValue = currentPrice.Split('￦')[1];
        var newValue = newPrice.Split('￦')[1];
        return double.Parse(currentValue) > double.Parse(newValue) ? CheaperPrice.Yes : CheaperPrice.No;
    }

    private static MonitoredItem CollectItemInfoFromHanstyleMobile(IEnumerable<HtmlNode> nodes, string url)
    {
        string? brand = GetValueForProperty(nodes, Constants.BrandProperty);
        var salePrice = GetValueForProperty(nodes, Constants.SalePriceProperty);
        var title = GetValueForProperty(nodes, Constants.TitleProperty);
        var img = GetValueForProperty(nodes, Constants.ImageProperty);
        var price = GetValueForProperty(nodes, Constants.PriceProperty);
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
            ShopName = ShopName.Hanstyle,
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
            ProductUrl = url,
            AvailableSizesAsString = $"\n{string.Join("  ", availableSizes!.Select(s => $"[ {s.Size} ]"))}",
            Id = UriService.GetProductIdValueFromUri(url),
            LinkToProduct = url
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

    private static async Task<string> GetStringContentFromProvidedUrl(string url)
    {
        using HttpClient client = new();
        //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        //client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
        using HttpResponseMessage response = await client.GetAsync(url);
        using HttpContent content = response.Content;

        return await content.ReadAsStringAsync();
    }
}
