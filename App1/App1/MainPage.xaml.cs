﻿using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Position = Xamarin.Forms.GoogleMaps.Position;

namespace App1
{
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        double commpas = 0;
        Polyline polyline = null;
        bool isCompas = false;
        bool isMove = false;
        Pin pin = new Pin();

        public MainPage()
        {
            InitializeComponent();
            Compass.ReadingChanged += ReadingChanged;
            map.MyLocationEnabled = true;
            map.UiSettings.MyLocationButtonEnabled = true;
            map.UiSettings.TiltGesturesEnabled = true;
            map.UiSettings.IndoorLevelPickerEnabled = true;
            map.UiSettings.ZoomControlsEnabled = false;
            map.UiSettings.ScrollGesturesEnabled = true;
            map.UiSettings.RotateGesturesEnabled = true;
            Init();
        }

        private async void Init()
        {
            CrossGeolocator.Current.PositionChanged += GpsTrecking;
            Plugin.Geolocator.Abstractions.Position position = await CrossGeolocator.Current.GetLastKnownLocationAsync();
            if (position != null)
            {
                map.InitialCameraUpdate = CameraUpdateFactory.NewCameraPosition(new CameraPosition(new Position(position.Latitude, position.Longitude), 20, commpas, 50));
            }
            isCompas = true;
        }

        bool isRefreshPolylines = true;
        private async void GpsTrecking(object sender, PositionEventArgs e)
        {
            await Task.Run(() =>
            {
                Position position = map.Polylines[0].Positions[0];
                if ((position.Latitude < e.Position.Latitude + 0.000020 && position.Latitude > e.Position.Latitude - 0.000020) && (position.Longitude < e.Position.Longitude + 0.000020 && position.Longitude > e.Position.Longitude - 0.000020))
                {
                    map.Polylines[0].Positions.Remove(position);
                    return;
                }
                if ((e.Position.Latitude - position.Latitude > 0.000040 || e.Position.Latitude - position.Latitude < -0.000040 || position.Longitude - e.Position.Longitude > 0.000040 || position.Longitude - e.Position.Longitude < -0.000040))
                {
                    if (isRefreshPolylines)
                    {
                        isRefreshPolylines = false;
                        RefreshPoly();
                    }
                }
            });
        }

        private async void ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            if (e.Reading.HeadingMagneticNorth - commpas > 2 || e.Reading.HeadingMagneticNorth - commpas < -2)
            {
                isMove = true;
                commpas = e.Reading.HeadingMagneticNorth;
                if (isCompas)
                {
                    Plugin.Geolocator.Abstractions.Position position = await CrossGeolocator.Current.GetLastKnownLocationAsync();
                    if (position != null)
                    {
                        await map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(new CameraPosition(new Position(position.Latitude, position.Longitude), 20, e.Reading.HeadingMagneticNorth, 70)), TimeSpan.FromMilliseconds(10));
                    }
                }
            }
            else
            {
                isMove = false;
            }
        }

        private void Map_MyLocationButtonClicked(object sender, MyLocationButtonClickedEventArgs e)
        {
            if (!Compass.IsMonitoring)
            {
                Compass.Start(SensorSpeed.UI);
            }
        }

        private async void Map_CameraMoveStarted(object sender, CameraMoveStartedEventArgs e)
        {
            if (Compass.IsMonitoring && e.IsGesture)
            {
                Compass.Stop();
            }
        }

        private async void RefreshPoly()
        {
            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
            }
            Xamarin.Forms.GoogleMaps.Polyline polyline = null;
            await Task.Run(async () =>
            {
                Plugin.Geolocator.Abstractions.Position position = await CrossGeolocator.Current.GetLastKnownLocationAsync();
                List<Location> locations = Mapmanager.GetPoint(position.Latitude, position.Longitude, map.Polylines[0].Positions.Last().Latitude, map.Polylines[0].Positions.Last().Longitude);
                polyline = new Xamarin.Forms.GoogleMaps.Polyline();

                foreach (var location in locations)
                {
                    polyline.Positions.Add(new Position(location.Latitude, location.Longitude));
                }
                polyline.StrokeColor = Color.Blue;
                polyline.StrokeWidth = 5f;
                map.Polylines.Clear();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    map.Polylines.Add(polyline);
                });
                if (!CrossGeolocator.Current.IsListening)
                {
                    isRefreshPolylines = true;
                    await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 1, true);
                }
            });
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
            }
            Xamarin.Forms.GoogleMaps.Polyline polyline = null;
            await Task.Run(async () =>
           {
               Plugin.Geolocator.Abstractions.Position position = await CrossGeolocator.Current.GetLastKnownLocationAsync();
               List<Location> locations = Mapmanager.GetPoint(position.Latitude, position.Longitude, streat: streetE.Text);
               polyline = new Xamarin.Forms.GoogleMaps.Polyline();

               foreach (var location in locations)
               {
                   polyline.Positions.Add(new Position(location.Latitude, location.Longitude));
               }
               polyline.StrokeColor = Color.Blue;
               polyline.StrokeWidth = 5f;
               map.Polylines.Clear();
               Device.BeginInvokeOnMainThread(async () =>
               {
                   map.Polylines.Add(polyline);
               });
               if (!CrossGeolocator.Current.IsListening)
               {
                   isRefreshPolylines = true;
                   await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 1, true);
               }
           });
        }

        private async void Map_MapClicked(object sender, MapClickedEventArgs e)
        {
            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
            }
            Xamarin.Forms.GoogleMaps.Polyline polyline = null;
            await Task.Run(async () =>
            {
                Plugin.Geolocator.Abstractions.Position position = await CrossGeolocator.Current.GetLastKnownLocationAsync();
                List<Location> locations = Mapmanager.GetPoint(position.Latitude, position.Longitude, e.Point.Latitude, e.Point.Longitude);
                polyline = new Xamarin.Forms.GoogleMaps.Polyline();

                foreach (var location in locations)
                {
                    polyline.Positions.Add(new Position(location.Latitude, location.Longitude));
                }
                polyline.StrokeColor = Color.Blue;
                polyline.StrokeWidth = 5f;
                map.Polylines.Clear();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    map.Polylines.Add(polyline);
                });
                if (!CrossGeolocator.Current.IsListening)
                {
                    isRefreshPolylines = true;
                    await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 1, true);
                }
            });
        }
    }
}
