namespace PriceMonitoringApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
    }

    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        if (args.Source == ShellNavigationSource.ShellSectionChanged)
        {
            var pageStack = Current.Navigation.NavigationStack;
            for (var i = pageStack.Count - 1; i >= 1; i--)
            {
                Navigation.RemovePage(pageStack[i]);
            }
        }
    }
}
