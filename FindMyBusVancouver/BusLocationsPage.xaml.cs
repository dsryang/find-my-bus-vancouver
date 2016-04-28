using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace FindMyBusVancouver
{
    public sealed partial class BusLocationsPage : Page
    {
        private BusData data;
        private BusLocation[] busLocations;
        private ErrorMessage errorMessage;

        public BusLocationsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            MapControlLocation.MapServiceToken = App.bingMapsApiKey;

            if (e.Parameter is BusData)
            {
                data = e.Parameter as BusData;
                TextBlockTitle.Text = "Bus Locations for Route " + data.routeNumber + " " + data.routeDirection;

                try
                {
                    await LoadBusPositions();
                    DisplayBusPositions();
                }
                catch (COMException ex)
                {
                    DisplayErrorMessage("Couldn't connect to the server. Please check your internet connection!");
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage("An error occurred when getting bus locations. Please go back and try again.");
                }
            }
            else
            {
                DisplayErrorMessage("An error occurred when getting bus locations. Please go back and try again.");
            }

            base.OnNavigatedTo(e);
        }

        private async Task LoadBusPositions()
        {
            Uri uri = new Uri("http://api.translink.ca/rttiapi/v1/buses?apikey=" + App.translinkApiKey + "&routeNo=" + data.routeNumber + "&stopNo=" + data.stopNumber);

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            string jsonString = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine(jsonString);

            try
            {
                busLocations = JsonConvert.DeserializeObject<BusLocation[]>(jsonString);
            }
            catch (JsonSerializationException e)
            {
                errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(jsonString);
            }
        }

        private async void DisplayBusPositions()
        {
            if (busLocations != null && busLocations.Length > 0)
            {
                TextBlockMessage.Text = "Last Updated: " + DateTime.Now.ToString("h:mm tt");

                // Set the map center to the position of the closest bus
                BasicGeoposition position = new BasicGeoposition();
                position.Latitude = busLocations[0].Latitude;
                position.Longitude = busLocations[0].Longitude;
                await MapControlLocation.TrySetViewAsync(new Geopoint(position), 17D);

                // Display markers for the position of each bus
                foreach (BusLocation bus in busLocations)
                {
                    MapIcon icon = new MapIcon();
                    icon.Location = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = bus.Latitude,
                        Longitude = bus.Longitude
                    });
                    icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                    icon.Title = "To " + bus.Destination;
                    MapControlLocation.MapElements.Add(icon);
                }
            }
            else if (errorMessage != null)
            {
                DisplayErrorMessage(errorMessage.Message);
            }
            else
            {
                DisplayErrorMessage("An error occurred when getting bus locations. Please go back and try again.");
            }
        }

        private void DisplayErrorMessage(string message)
        {
            TextBlockMessage.Visibility = Visibility.Collapsed;
            MapControlLocation.Visibility = Visibility.Collapsed;

            TextBlockError.Text = message;
            TextBlockError.Visibility = Visibility.Visible;
        }
    }
}
