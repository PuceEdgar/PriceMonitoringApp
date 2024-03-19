using PriceMonitoringLibrary.Models;
using System.Diagnostics.CodeAnalysis;

namespace PriceMonitoringApp;

public class SizeDetailsComparer : IEqualityComparer<SizeDetails>
{
    public bool Equals(SizeDetails? x, SizeDetails? y)
    {
        return x!.Availability!.Equals(y!.Availability, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode([DisallowNull] SizeDetails obj)
    {
        return obj.Availability!.GetHashCode();
    }
}
