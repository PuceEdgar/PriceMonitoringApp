using System.Web;

namespace PriceMonitoringLibrary.Services;

public static class UriService
{
    public static string GetProductIdValueFromUri(string url)
    {
        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var productId = query.Get(Constants.ProductIdKey);

        return productId ?? string.Empty;
    }
}
