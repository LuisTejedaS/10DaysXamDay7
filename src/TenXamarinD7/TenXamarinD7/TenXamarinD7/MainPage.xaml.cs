﻿using Newtonsoft.Json;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using SQLite;
using System;
using System.Linq;
using System.Net.Http;
using TenXamarinD7.Model;
using Xamarin.Forms;

namespace TenXamarinD7
{
    public partial class MainPage : ContentPage
    {

        IGeolocator locator = CrossGeolocator.Current;
        Position position;

        public string YOUR_CLIENT_SECRET = "QGRHJUZ0N0L2ZZXQ4A55FUUXUNQ1DET4FRWGNZMX4NVCL5WV";
        public string YOUR_CLIENT_ID = "WNMW4P1TT35SMAWQQAIXVLGWAKTCRITZFJ2TJASWUVE141YD";

        public MainPage()
        {
            InitializeComponent();

            locator.PositionChanged += Locator_PositionChanged;
        }

        void Locator_PositionChanged(object sender, PositionEventArgs e)
        {
            position = e.Position; // this uses the global variable defined earlier
        }

        private void CheckIfShouldBeEnabled()
        {
            saveButton.IsEnabled = false;
            if (!string.IsNullOrWhiteSpace(titleEntry.Text) && !string.IsNullOrWhiteSpace(contentEditor.Text))
                saveButton.IsEnabled = true;
        }

        private void TitleEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfShouldBeEnabled();
        }

        private void ContentEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfShouldBeEnabled();
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {

            Experience newExperience = new Experience()
            {
                Title = titleEntry.Text,
                Content = contentEditor.Text,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                VenueName = venueNameLabel.Text,
                VenueCategory = venueCategoryLabel.Text,
                VenueLat = float.Parse(venueCoordinatesLabel.Text.Split(',')[0]),
                VenueLng = float.Parse(venueCoordinatesLabel.Text.Split(',')[1])
            };
            var inserted = 0;

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabasePath))
            {
                conn.CreateTable<Experience>();
                inserted = conn.Insert(newExperience);
            }

            if (inserted > 0)
            {
                titleEntry.Text = string.Empty;
                contentEditor.Text = string.Empty;
            }
            else
            {
                DisplayAlert("Error", "There was an error inserting the experience", "ok");
            }
            titleEntry.Text = string.Empty;
            contentEditor.Text = string.Empty;
            venueNameLabel.Text = string.Empty;
            venueCategoryLabel.Text = string.Empty;
            venueCoordinatesLabel.Text = string.Empty;

        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        async void SearchEntry_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(searchEntry.Text))
            { 
                string url = $"https://api.foursquare.com/v2/venues/search?ll={position.Latitude},{position.Longitude}&radius=500&query={searchEntry.Text}&limit=3&client_id={YOUR_CLIENT_ID}&client_secret={YOUR_CLIENT_SECRET}&v={DateTime.Now.ToString("yyyyMMdd")}";
                // added using System.Net.Http;
                using (HttpClient client = new HttpClient())
                {
                    // made the method async
                    string json = await client.GetStringAsync(url);

                    // added using Newtonsoft.Json;
                    Search searchResult = JsonConvert.DeserializeObject<Search>(json);
                    venuesListView.IsVisible = true;
                    venuesListView.ItemsSource = searchResult.response.venues;
                }
            }
            else
            {
                venuesListView.IsVisible = false;
            }
        }

        void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            if (venuesListView.SelectedItem != null)
            {
                selectedVenueStackLayout.IsVisible = true;
                searchEntry.Text = string.Empty;
                venuesListView.IsVisible = false;

                Venue selectedVenue = venuesListView.SelectedItem as Venue;
                venueNameLabel.Text = selectedVenue.name;
                venueCategoryLabel.Text = selectedVenue.categories.FirstOrDefault()?.name;
                venueCoordinatesLabel.Text = $"{selectedVenue.location.lat:0.000}, {selectedVenue.location.lng:0.000}";
            }
            else
            {
                selectedVenueStackLayout.IsVisible = false;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetLocationPermission();
        }

        private async void GetLocationPermission()
        {
            // added using Plugin.Permissions;
            // added using Plugin.Permissions.Abstractions;
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
            if (status != PermissionStatus.Granted)
            {
                // Not granted, request permission
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.LocationWhenInUse))
                {
                    // This is not the actual permission request
                    await DisplayAlert("Need your permission", "We need to access your location", "Ok");
                }

                // This is the actual permission request
                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
                if (results.ContainsKey(Permission.LocationWhenInUse))
                    status = results[Permission.LocationWhenInUse];
            }



            // Already granted (maybe), go on
            if (status == PermissionStatus.Granted)
            {
                // Granted! Get the location
                GetLocation();
            }
            else
            {
                await DisplayAlert("Access to location denied", "We don't have access to your location", "Ok");
            }
        }



        private async void GetLocation()
        {
            position = await locator.GetPositionAsync();
            await locator.StartListeningAsync(TimeSpan.FromMinutes(1), 500);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            locator.StopListeningAsync();
        }

    }
}
