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
        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
        using HttpResponseMessage response = await client.GetAsync(url);
        using HttpContent content = response.Content;

        // ... Read the string.
        string result = await content.ReadAsStringAsync();
        HtmlDocument doc = new();
        doc.LoadHtml(result);
        var nodes = doc.DocumentNode.Descendants(0);

        if (nodes is null || nodes.Count() < 700)
        {
            return null;
        }

        MonitoredItem? item = url switch
        {
            string a when a.Contains(Constants.MobileHanstyleDomainName, StringComparison.CurrentCultureIgnoreCase) => CollectItemInfoFromHanstyleMobile(nodes, url),
            string a when a.Contains(Constants.HanstyleDomainName, StringComparison.CurrentCultureIgnoreCase) => CollectItemInfoFromHanstyle(nodes, url),
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
            var newItemData = await GetItemFromUrl(item.ShareUrl);
            if (item.Price != newItemData.Price)
            {
                detailsChanged = true;
                item.PreviousPrice = item.Price;
                item.PriceHistory.Add(new HistoryDetails { Price = item.Price });
                item.Price = newItemData.Price;
            }
            var result = item.AvailableSizes.Except(newItemData.AvailableSizes, new SizeDetailsComparer())?.ToList();
            if (result.Count > 0)
            {
                detailsChanged = true;
                item.AvailableSizes = newItemData.AvailableSizes;
            }
        }

        if (detailsChanged)
        {
            await FileService.SaveToFile(new ObservableCollection<MonitoredItem>(monitoredItems));
        }

        return detailsChanged;
    }

    private static MonitoredItem CollectItemInfoFromHanstyle(IEnumerable<HtmlNode> nodes, string url)
    {
        //product name: class="pdt_name" class="font_w_40 fo_14"
        var name = nodes.FirstOrDefault(n => n.HasClass("pdt_name"))?.ChildNodes[3].InnerText;
        //product price: class="pdt_price" id="price_discount" old price class="col_gray_dl fo_16" discount class="disp_ib fo_22 font_w_70"
        //for mobile use class="price" and class="sale"
        var mobilepriceNodeChildren = nodes.FirstOrDefault(n => n.HasClass("price"))?.ChildNodes;
        var discountPrice = mobilepriceNodeChildren?.FirstOrDefault(c => c.Name == "span")?.NextSibling.InnerText.Trim();
        var originalPrice = mobilepriceNodeChildren?.FirstOrDefault(c => c.Name == "del")?.InnerText.Trim();
        var discountPercent = mobilepriceNodeChildren?.FirstOrDefault(c => c.Name == "span")?.InnerText.Trim();
        //var priceNodeChildren = nodes.FirstOrDefault(n => n.HasClass("pdt_price"))?.ChildNodes;
        //var discountPrice = priceNodeChildren?.FirstOrDefault(c => c.Id == "price_discount")?.InnerText;
        //var originalPrice = priceNodeChildren?.FirstOrDefault(c => c.Name == "del")?.InnerText;
        //var discountPercent = priceNodeChildren?.FirstOrDefault(c => c.Name == "span")?.InnerText;
        //product special price?: class="special_price"
        //HtmlNode specialPriceNode = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("special_price")).FirstOrDefault();
        //size options: class="sizelist mt-1" class="con"
        HtmlNode sizeNode = nodes.FirstOrDefault(n => n.HasClass("sizelist"));
        var sizes = sizeNode?.Descendants(0).Where(d => d.HasClass("con")).Select(s => s.InnerText).ToList();

        MonitoredItem item = new()
        {
            Description = name,
            Price = discountPrice,
            OriginalPrice = originalPrice,
            DiscountPercent = discountPercent,
            //Sizes = sizes
        };

        return item;
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

        var isSoldOut = IsItemSoldOut(nodes, allSizes);
        var mobilepriceNodeChildren = nodes.FirstOrDefault(n => n.HasClass("price"))?.ChildNodes;
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
}
