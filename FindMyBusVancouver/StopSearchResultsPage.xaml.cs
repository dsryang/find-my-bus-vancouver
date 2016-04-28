using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace FindMyBusVancouver
{
    public sealed partial class StopSearchResultsPage : Page
    {
        private double longitude;
        private double latitude;
        private StopSearchResult[] searchResults;
        private ErrorMessage errorMessage;
        private ObservableCollection<StopSearchResultItem> collection;

        public StopSearchResultsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Geopoint)
            {
                Geopoint point = e.Parameter as Geopoint;
                longitude = point.Position.Longitude;
                latitude = point.Position.Latitude;

                collection = new ObservableCollection<StopSearchResultItem>();
                ListViewSearchResults.ItemsSource = collection;

                try
                {
                    await LoadSearchResults();
                    DisplaySearchResults();
                }
                catch (COMException ex)
                {
                    DisplayErrorMessage("Couldn't connect to the server. Please check your internet connection!");
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage("An error occurred when getting stop search results. Please go back and try again.");
                }
            }
            else
            {
                DisplayErrorMessage("An error occurred when getting stop search results. Please go back and try again.");
            }
        }

        private async Task LoadSearchResults()
        {
            Uri uri = new Uri("http://api.translink.ca/rttiapi/v1/stops?apikey=" + App.translinkApiKey + "&lat=" + latitude.ToString("N6") + "&long=" + longitude.ToString("N6"));

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            string jsonString = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(jsonString);

            try
            {
                searchResults = JsonConvert.DeserializeObject<StopSearchResult[]>(jsonString);
            }
            catch (JsonSerializationException e)
            {
                errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(jsonString);
            }
        }

        private void DisplaySearchResults()
        {
            if (searchResults != null && searchResults.Length > 0)
            {
                TextBlockMessage.Visibility = Visibility.Collapsed;
                collection.Clear();

                foreach (StopSearchResult stop in searchResults)
                {
                    StopSearchResultItem item = new StopSearchResultItem();
                    item.number = "Stop #" + stop.StopNo;
                    item.distance = stop.Distance + " m away";
                    item.name = stop.Name;
                    item.routes = "Routes: " + stop.Routes;
                    collection.Add(item);
                }
            }
            else if (errorMessage != null)
            {
                DisplayErrorMessage(errorMessage.Message);
            }
            else
            {
                DisplayErrorMessage("An error occurred when getting stop search results. Please go back and try again.");
            }
        }

        private void DisplayErrorMessage(string message)
        {
            TextBlockMessage.Visibility = Visibility.Collapsed;
            ListViewSearchResults.Visibility = Visibility.Collapsed;

            TextBlockError.Text = message;
            TextBlockError.Visibility = Visibility.Visible;
        }

        private void ListViewSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string stopNumber = searchResults[ListViewSearchResults.SelectedIndex].StopNo.ToString();
            Frame.Navigate(typeof(StopEstimatesPage), stopNumber);
        }
    }
}
