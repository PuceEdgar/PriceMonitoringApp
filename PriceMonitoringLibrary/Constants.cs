
namespace PriceMonitoringLibrary;

internal static class Constants
{
    public const string BrandProperty = "recopick:brand";
    public const string SalePriceProperty = "recopick:sale_price";
    public const string TitleProperty = "recopick:title";
    public const string ImageProperty = "recopick:image";
    public const string PriceProperty = "recopick:price";
    public const string UrlProperty = "tas:productUrl";
    public const string AvailabilityProperty = "recopick:availability";
    public const string OptionSelectBoxTag = "optionSelectBox";
    public const string SoldOut = "[품절]";
    public const string Meta = "meta";
    public const string Property = "property";
    public const string MobileHanstyleDomainName = "mshop.ihanstyle";
    public const string MusinsaDomainName = "musinsa";
    public const string HanstyleDomainName = "hanstyle";
    public const string MusinsaRemainingSizeUrl = "";
    public const string FileName = "ItemData.json";
    public const string ProductIdKey = "PROD_CD";

    public static string GetMusinsaItemDetailUrl(string productId)
    {
        return $"https://goods-detail.musinsa.com/goods/{productId}";
    }

    public static string GetMusinsaRemainingSizeUrl(string productId)
    {
        return $"https://goods-detail.musinsa.com/goods/{productId}/options?goodsSaleType=SALE";
    }

    public static string? GetMusinsaImageUrl(string thumbnailUrl)
    {
        return $"https://image.msscdn.net{thumbnailUrl}";
    }
}
