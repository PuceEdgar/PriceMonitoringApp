using CommunityToolkit.Maui.Views;
using PriceMonitoringLibrary;
using System.Collections.ObjectModel;
using SelectionChangedEventArgs = Microsoft.Maui.Controls.SelectionChangedEventArgs;

namespace PriceMonitoringApp
{
    public partial class MainPage : ContentPage
    {
        ObservableCollection<MonitoredItem> MonitoredItems = [];
        readonly IPriceCheckerService PriceCheckerService;

        public MainPage(IPriceCheckerService priceCheckerService)
        {
            InitializeComponent();
            ItemExistsLabel.IsVisible = false;
            FailedToLoadItem.IsVisible = false;
            PriceCheckerService = priceCheckerService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MonitoredItems = new ObservableCollection<MonitoredItem>(await FileHelper.GetSavedItemData());
            itemsList.ItemsSource = MonitoredItems;
            var isRunning = ForegroundServiceHelper.IsForegroundServiceRunning();
            ServiceButton.Text = isRunning ? "Stop service" : "Start Service";
        }

        private async void OnAddNewItemClicked(object sender, EventArgs e)
        {
            await AddItemFromUrl();
        }

        private async void OnEntryCompleted(object sender, EventArgs e)
        {
            await AddItemFromUrl();
        }

        private async Task AddItemFromUrl()
        {
            if (string.IsNullOrWhiteSpace(itemUrl.Text))
            {
                await Application.Current.MainPage.DisplayAlert("Adding new item", "Link to item was not provided. Please provide link again.", "OK");
                return;
            }
            if (MonitoredItems.Any(mi => mi.ShareUrl == itemUrl.Text))
            {
                ItemExistsLabel.IsVisible = true;
                return;
            }

            var item = await DataScraper.GetItemFromUrl(itemUrl.Text);

            if (item is null)
            {
                FailedToLoadItem.IsVisible = true;
                return;
            }
            item.ShareUrl = itemUrl.Text;

            MonitoredItems.Add(item);

            await FileHelper.SaveToFile(MonitoredItems);
            itemUrl.Text = null;
            itemUrl.Unfocus();
        }

        private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (itemsList.SelectedItem is null)
            {
                return;
            }

            await this.ShowPopupAsync(new ItemDetailsContentView(e.CurrentSelection.FirstOrDefault() as MonitoredItem));
            itemsList.SelectedItem = null;
        }

        private async void DeleteItem_Clicked(object sender, EventArgs e)
        {
            var context = (sender as Button)?.BindingContext;

            if (context is not MonitoredItem itemToDelete)
            {
                return;
            }

            var isDeleteConfirmed = await Application.Current.MainPage.DisplayAlert("Deleting item", $"This item will be deleted\n {itemToDelete?.Brand}\n {itemToDelete?.Descripton}", "Delete", "Cancel");
            if (isDeleteConfirmed && MonitoredItems.Remove(itemToDelete!))
            {
                await FileHelper.SaveToFile(MonitoredItems);
            }
        }

        private async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            var hasSomethingChanged = await DataScraper.CheckIfItemDetailsHaveCHanged();

            if (hasSomethingChanged)
            {
                await NotificationService.ShowGeneralChangeNotification();
            }
            //TODO:
            //implement proper logic to notify for changes
            //if ((priceChanged && sizeAvailabilityChanged) || changedItems.Count > 1)
            //{
            //    ShowGeneralChangeNotification();
            //}
            //else if (priceChanged && changedItems.Count == 1)
            //{
            //    ShowPriceChangeNotification(changedItems[0]);
            //}
            //else if (sizeAvailabilityChanged && changedItems.Count == 1) { ShowSizeAvailabilityChangeNotification(changedSizeInfo[0]); }
        }

        private void ServiceButton_Clicked(object sender, EventArgs e)
        {
           var isRunning = PriceCheckerService.ToggleService();
            ServiceButton.Text = isRunning ? "Stop service" : "Start Service";
        }
    }
}
