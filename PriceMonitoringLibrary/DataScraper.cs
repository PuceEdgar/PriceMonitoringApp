using HtmlAgilityPack;
using PriceMonitoringApp;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PriceMonitoringLibrary;

public static class DataScraper
{
    public static async Task<MonitoredItem> GetItemFromUrl(string url)
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

        if (nodes is null)
        {
            return new MonitoredItem();
        }

        MonitoredItem item = url switch
        {
            string a when a.Contains("mshop.ihanstyle", StringComparison.CurrentCultureIgnoreCase) => CollectItemInfoFromHanstyleMobile(nodes),
            string a when a.Contains("hanstyle", StringComparison.CurrentCultureIgnoreCase) => CollectItemInfoFromHanstyle(nodes),
            //string b when b.Contains("musinsa", StringComparison.CurrentCultureIgnoreCase) => CollectItemInfoFromMusinsa(nodes),
            _ => new()
        };

        return item;
    }

    public static async Task<bool> CheckIfItemDetailsHaveCHanged()
    {
        var detailsChanged = false;
        var monitoredItems = await FileHelper.GetSavedItemData();

        foreach (var item in monitoredItems)
        {
            //item.Price = "90000";
            var newItemData = await GetItemFromUrl(item.ShareUrl);
            if (item.Price != newItemData.Price)
            {
                detailsChanged = true;
                item.PreviousPrice = item.Price;
                item.PriceHistory.Add(new HistoryDetails { Price = item.Price });
                item.Price = newItemData.Price;
            }
            var result = item.AvailableSizes.Except(newItemData.AvailableSizes, new SizeDetailsComparer())?.ToList();
            if (result is not null)
            {
                detailsChanged = true;
                item.AvailableSizes = newItemData.AvailableSizes;
                // changedSizeInfo.AddRange(result);
            }
        }

        if (detailsChanged)
        {
            await FileHelper.SaveToFile(new ObservableCollection<MonitoredItem>(monitoredItems));
        }

        return detailsChanged;
    }

    private static MonitoredItem CollectItemInfoFromHanstyle(IEnumerable<HtmlNode> nodes)
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
            Descripton = name,
            Price = discountPrice,
            OriginalPrice = originalPrice,
            DiscountPercent = discountPercent,
            //Sizes = sizes
        };

        return item;
    }

    private static MonitoredItem CollectItemInfoFromHanstyleMobile(IEnumerable<HtmlNode> nodes)
    {
        var brand = nodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("recopick:brand"))).Attributes[1].Value;
        var salePrice = nodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("recopick:sale_price"))).Attributes[1].Value;
        var title = nodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("recopick:title"))).Attributes[1].Value;
        var img = nodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("recopick:image"))).Attributes[1].Value;
        var price = nodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("recopick:price"))).Attributes[1].Value;
        var productUrl = nodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("tas:productUrl"))).Attributes[1].Value;
        var options = nodes.FirstOrDefault(n => n.HasClass("optionSelectBox"))?.ChildNodes?.FirstOrDefault(n => n.Name == "ul")?.ChildNodes.Where(n => n.Name == "li");
        var allSizes = new List<SizeDetails>();
        var availableSizes = new List<SizeDetails>();

        foreach (var option in options)
        {
            var size = option.ChildNodes[1]?.InnerText;
            var availability = option.ChildNodes.Count > 4 ? option.ChildNodes[3]?.InnerText.Trim() : "Many";
            allSizes.Add(new SizeDetails(size, availability));
            if (!availability.Contains("[품절]"))
            {
                availableSizes.Add(new SizeDetails(size, availability));
            }
        }

        var isSoldOut = IsItemSoldOut(nodes, allSizes);
        //var sale_price = nodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("recopick:sale_price")));
        //product name: class="pdt_name" class="font_w_40 fo_14"
        var name = nodes.FirstOrDefault(n => n.HasClass("pdt_name"))?.ChildNodes[3].InnerText;
        //product price: class="pdt_price" id="price_discount" old price class="col_gray_dl fo_16" discount class="disp_ib fo_22 font_w_70"
        //for mobile use class="price" and class="sale"
        var mobilepriceNodeChildren = nodes.FirstOrDefault(n => n.HasClass("price"))?.ChildNodes;
        var discountPrice = mobilepriceNodeChildren?.FirstOrDefault(c => c.Name == "span")?.NextSibling.InnerText.Trim();
        var originalPrice = mobilepriceNodeChildren?.FirstOrDefault(c => c.Name == "del")?.InnerText.Trim();
        var discountPercent = mobilepriceNodeChildren?.FirstOrDefault(c => c.Name == "span")?.InnerText.Trim();

        //HtmlNode sizeNode = nodes.FirstOrDefault(n => n.HasClass("sizelist"));
        //var sizes = sizeNode?.Descendants(0).Where(d => d.HasClass("con")).Select(s => s.InnerText).ToList();

        MonitoredItem item = new()
        {
            Brand = brand,
            Descripton = title,
            Price = FormatStringToCurrency(salePrice),
            InitialProductPrice = FormatStringToCurrency(price),
            OriginalPrice = FormatStringToCurrency(price),
            DiscountPercent = discountPercent,
            AllSizes = allSizes,
            AvailableSizes = availableSizes,
            ImageUrl = $"https:{img}",
            IsSoldOut = isSoldOut,
            ProductUrl = productUrl,
            AvailableSizesAsString = $"\n{string.Join("  ", availableSizes?.Select(s => $"[ {s.Size} ]"))}"
        };

        return item;
    }

    private static string FormatStringToCurrency(string price)
    {
        return $"￦{string.Format(CultureInfo.InvariantCulture, "{0:n0}", int.Parse(price))}";
    }

    private static bool IsItemSoldOut(IEnumerable<HtmlNode> nodes, List<SizeDetails> sizes)
    {
        var soldOutMetaExists = nodes.Any(n => n.Name == "meta" && n.Attributes.AttributesWithName("property").Any(a => a.Value.Equals("recopick:availability")));
        var noSizeLeft = sizes!.TrueForAll(s => s.Availability.Trim().Contains("[품절]"));
        return noSizeLeft || soldOutMetaExists;
    }

    
}
