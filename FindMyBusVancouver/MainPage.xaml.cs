using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FindMyBusVancouver
{
    public sealed partial class MainPage : Page
    {
        private Geopoint mapLocationPoint;
        private string chosenSuggestion;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MapControlLocation.MapServiceToken = App.bingMapsApiKey;
            ShowDefaultLocation();
        }

        private void ButtonFindByStop_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxStop.Text.Length > 0)
            {
                TextBlockError.Visibility = Visibility.Collapsed;
                Frame.Navigate(typeof(StopEstimatesPage), TextBoxStop.Text);
            }
            else
            {
                TextBlockError.Visibility = Visibility.Visible;
            }
        }

        private async void ShowDefaultLocation()
        {
            // Defaults to Vancouver
            BasicGeoposition position = new BasicGeoposition();
            position.Latitude = 49.2555719;
            position.Longitude = -123.1290476;
            mapLocationPoint = new Geopoint(position);

            await MapControlLocation.TrySetViewAsync(mapLocationPoint, 13D);
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Length > 2)
                {
                    sender.ItemsSource = await getMapSuggestionsAsync(sender.Text, mapLocationPoint);
                }
                else
                {
                    sender.ItemsSource = new List<MapSuggestionItem> { };
                }
            }
            else if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            {
                sender.Text = chosenSuggestion;
            }
        }

        public static async Task<List<MapSuggestionItem>> getMapSuggestionsAsync(String query, Geopoint hintPoint)
        {
            List<MapSuggestionItem> locations = new List<MapSuggestionItem>();
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(query, hintPoint, 20);

            if (result.Status != MapLocationFinderStatus.Success)
            {
                return locations;
            }

            foreach (var location in result.Locations)
            {
                if (location.Address.Country == null || !location.Address.Country.Equals("Canada"))
                {
                    continue;
                }

                MapSuggestionItem suggestion = new MapSuggestionItem();
                string text = "";

                if (location.Address.StreetNumber != null && location.Address.StreetNumber.Length > 0)
                {
                    text += location.Address.StreetNumber;
                }

                if (location.Address.Street != null && location.Address.Street.Length > 0)
                {
                    if (text.Length > 0)
                    {
                        text += " ";
                    }
                    text += location.Address.Street;
                }

                if (location.Address.Neighborhood != null && location.Address.Neighborhood.Length > 0)
                {
                    if (text.Length > 0)
                    {
                        text += ", ";
                    }
                    text += location.Address.Neighborhood;
                }

                if (location.Address.Town != null && location.Address.Town.Length > 0)
                {
                    if (text.Length > 0)
                    {
                        text += ", ";
                    }
                    text += location.Address.Town;
                }

                if (text.Length == 0)
                {
                    text = location.Address.FormattedAddress;
                }

                suggestion.name = text;
                suggestion.point = location.Point;
                locations.Add(suggestion);
            }

            return locations;
        }

        private async void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            MapSuggestionItem selected = args.SelectedItem as MapSuggestionItem;
            chosenSuggestion = selected.name;
            mapLocationPoint = selected.point;
            await MapControlLocation.TrySetViewAsync(mapLocationPoint, 15D);
        }

        private void ButtonFindStopNumber_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StopSearchResultsPage), mapLocationPoint);
        }
    }
}
