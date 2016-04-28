using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class StopEstimatesPage : Page
    {
        private string stopNumber;
        private StopEstimate[] stopEstimates;
        private ErrorMessage errorMessage;
        private ObservableCollection<StopEstimateItem> collection;

        public StopEstimatesPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string)
            {
                stopNumber = e.Parameter.ToString();
                TextBlockTitle.Text = "Departure Times for Stop #" + stopNumber;

                collection = new ObservableCollection<StopEstimateItem>();
                ListViewStopEstimates.ItemsSource = collection;

                try
                {
                    await LoadDepartureTimes();
                    DisplayDepartureTimes();
                }
                catch (COMException ex)
                {
                    DisplayErrorMessage("Couldn't connect to the server. Please check your internet connection!");
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage("An error occurred when getting stop estimates. Please go back and try again.");
                }
            }
            else
            {
                DisplayErrorMessage("An error occurred when getting stop estimates. Please go back and try again.");
            }

            base.OnNavigatedTo(e);
        }

        private async Task LoadDepartureTimes()
        {
            Uri uri = new Uri("http://api.translink.ca/rttiapi/v1/stops/" + stopNumber + "/estimates?apikey=" + App.translinkApiKey + "&count=3");

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            string jsonString = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(jsonString);

            try
            {
                stopEstimates = JsonConvert.DeserializeObject<StopEstimate[]>(jsonString);
            }
            catch (JsonSerializationException e)
            {
                errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(jsonString);
            }
        }

        private void DisplayDepartureTimes()
        {
            if (stopEstimates != null && stopEstimates.Length > 0)
            {
                TextBlockMessage.Text = "Last Updated: " + DateTime.Now.ToString("h:mm tt");
                collection.Clear();

                foreach (StopEstimate stop in stopEstimates)
                {
                    StopEstimateItem item = new StopEstimateItem();
                    item.number = stop.RouteNo + " " + stop.Direction;
                    item.name = stop.RouteName;

                    if (stop.Schedules.Length >= 1)
                    {
                        int countdown = stop.Schedules[0].ExpectedCountdown;
                        if (countdown <= 0)
                        {
                            item.estimate1 = "Due";
                        }
                        else
                        {
                            item.estimate1 = countdown + " min";
                        }
                        item.destination1 = stop.Schedules[0].Destination;
                    }
                    else
                    {
                        item.estimate1 = "";
                        item.destination1 = "";
                    }

                    if (stop.Schedules.Length >= 2)
                    {
                        int countdown = stop.Schedules[1].ExpectedCountdown;
                        if (countdown <= 0)
                        {
                            item.estimate2 = "Due";
                        }
                        else
                        {
                            item.estimate2 = countdown + " min";
                        }
                        item.destination2 = stop.Schedules[1].Destination;
                    }
                    else
                    {
                        item.estimate2 = "";
                        item.destination2 = "";
                    }

                    if (stop.Schedules.Length >= 3)
                    {
                        int countdown = stop.Schedules[2].ExpectedCountdown;
                        if (countdown <= 0)
                        {
                            item.estimate3 = "Due";
                        }
                        else
                        {
                            item.estimate3 = countdown + " min";
                        }
                        item.destination3 = stop.Schedules[2].Destination;
                    }
                    else
                    {
                        item.estimate3 = "";
                        item.destination3 = "";
                    }

                    collection.Add(item);
                }
            }
            else if (errorMessage != null)
            {
                DisplayErrorMessage(errorMessage.Message);
            }
            else
            {
                DisplayErrorMessage("An error occurred when getting stop estimates. Please go back and try again.");
            }
        }

        private void DisplayErrorMessage(string message)
        {
            TextBlockMessage.Visibility = Visibility.Collapsed;
            ListViewStopEstimates.Visibility = Visibility.Collapsed;

            TextBlockError.Text = message;
            TextBlockError.Visibility = Visibility.Visible;
        }

        private void ListViewStopEstimates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BusData data = new BusData();
            data.stopNumber = stopNumber;
            data.routeNumber = stopEstimates[ListViewStopEstimates.SelectedIndex].RouteNo;
            data.routeDirection = stopEstimates[ListViewStopEstimates.SelectedIndex].Direction;

            Frame.Navigate(typeof(BusLocationsPage), data);
        }
    }
}
