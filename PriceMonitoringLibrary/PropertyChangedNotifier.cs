using System.ComponentModel;

namespace PriceMonitoringLibrary;

public class PropertyChangedNotifier : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName]
            string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}