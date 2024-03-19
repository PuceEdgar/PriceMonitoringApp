using System.Web;

namespace PriceMonitoringLibrary.Services;

public static class UriService
{
    public static string GetProductCodeValueFromUri(string url)
    {
        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var productCode = query.Get(Constants.ProductCodeKey);

        return productCode ?? string.Empty;
    }
}
