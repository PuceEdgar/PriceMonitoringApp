
using CommunityToolkit.Maui.Views;
using Plugin.LocalNotification;
using PriceMonitoringLibrary;
using System.Collections.ObjectModel;
using System.Text;
using SelectionChangedEventArgs = Microsoft.Maui.Controls.SelectionChangedEventArgs;

namespace PriceMonitoringApp
{
    public partial class MainPage : ContentPage
    {
        ObservableCollection<MonitoredItem> MonitoredItems;
        IPriceCheckerService PriceCheckerService;

        public MainPage(IPriceCheckerService priceCheckerService)
        {
            InitializeComponent();
            ItemExistsLabel.IsVisible = false;
            FailedToLoadItem.IsVisible = false;
            PriceCheckerService = priceCheckerService;
            //if (CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
            //{
            //    RequestPermissions(notiPermission, requestNotification);
            //}
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MonitoredItems = new ObservableCollection<MonitoredItem>(await FileHelper.GetSavedItemData());
            itemsList.ItemsSource = MonitoredItems;
            var isRunning = ForegroundServiceHelper.IsForegroundServiceRunning();
            ServiceButton.Text = isRunning ? "Stop service" : "Start Service";
            //PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            //PermissionStatus status2 = await Permissions.RequestAsync<Permissions.PostNotifications>();

        }

        private async void OnAddNewItemClicked(object sender, EventArgs e)
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

            var soldOutUrl = "https://mshop.ihanstyle.com/product.do?cmd=getProductDetail&PROD_CD=SE041_2023_461715824&ITHR_CD=IT9999";
            var okUrl = "https://mshop.ihanstyle.com/product.do?cmd=getProductDetail&PROD_CD=SE013_2023_462304643&ITHR_CD=HS13_01";
            var otheri = "https://mshop.ihanstyle.com/product.do?cmd=getProductDetail&PROD_CD=SE013_2023_465448900&ITHR_CD=HS13_01";
            var item = await DataScraper.GetItemFromUrl(itemUrl.Text);

            if (item == null)
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

        //void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    string oldText = e.OldTextValue;
        //    string newText = e.NewTextValue;
        //    string myText = itemUrl.Text;
        //}

        private async void OnEntryCompleted(object sender, EventArgs e)
        {
            await AddItemFromUrl();
        }

        private async void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (itemsList.SelectedItem == null)
            {
                return;
            }
            await this.ShowPopupAsync(new ItemDetailsContentView(e.CurrentSelection.FirstOrDefault() as MonitoredItem));
            itemsList.SelectedItem = null;
        }

        private async void DeleteItem_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var itemToDelete = button?.BindingContext as MonitoredItem;
            var isDeleteConfirmed = await Application.Current.MainPage.DisplayAlert("Deleting item", $"This item will be deleted\n {itemToDelete.Brand}\n {itemToDelete.Descripton}", "Delete", "Cancel");
            if (isDeleteConfirmed && MonitoredItems.Remove(itemToDelete))
            {
                await FileHelper.SaveToFile(MonitoredItems);
            }
        }

        private async void HistoryButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button?.BindingContext as MonitoredItem;
            var message = new StringBuilder();

            item.PriceHistory.ForEach(h =>  message.AppendLine($"{h.Date} - {h.Price}"));
            await Application.Current.MainPage.DisplayAlert("Price History", message.ToString(), "Close");
        }

        private async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            //var priceChanged = false;
            //var sizeAvailabilityChanged = false;
            //List<MonitoredItem> changedItems = [];
            //List<SizeDetails> changedSizeInfo = [];
            var hasSomethingChanged = await DataScraper.CheckIfItemDetailsHaveCHanged();


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
            if (hasSomethingChanged)
            {
                ShowGeneralChangeNotification();                
            }            
        }

        

        private static async void ShowGeneralChangeNotification()
        {

            var request = new NotificationRequest
            {
                Title = "Product details changed",
                Description = $"Item details have changed!"
            };

            await LocalNotificationCenter.Current.Show(request);
        }

        //private static async void ShowPriceChangeNotification(MonitoredItem monitoredItem)
        //{

        //    var request = new NotificationRequest
        //    {
        //        Title = "Product details changed",
        //        Description = $"Price has changed!\n Old price: {monitoredItem.PriceHistory.LastOrDefault()}\n New price: {monitoredItem.Price}"
        //    };

        //   await LocalNotificationCenter.Current.Show(request);
        //}

        //private static async void ShowSizeAvailabilityChangeNotification(SizeDetails item)
        //{

        //    var request = new NotificationRequest
        //    {
        //        Title = "Product details changed",
        //        Description = $"Size availability has changed!\n Size: {item.Size} Availability: {item.Availability}"
        //    };

        //    await LocalNotificationCenter.Current.Show(request);
        //}

        private void ServiceButton_Clicked(object sender, EventArgs e)
        {
           var isRunning = PriceCheckerService.ToggleService();
            ServiceButton.Text = isRunning ? "Stop service" : "Start Service";
        }

        
    }
}
