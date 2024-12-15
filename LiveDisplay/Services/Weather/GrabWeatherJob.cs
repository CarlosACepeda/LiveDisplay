using Android.App;
using Android.Content;
using Android.Locations;
using Android.Nfc;
using AndroidX.Work;
using Java.Util;
using LiveDisplay.Misc;
using System;

namespace LiveDisplay.Services.Weather
{
    public class GrabWeatherJob : Worker
    {
        public const string Tag = "GrabWeatherJob";
        public GrabWeatherJob(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {

        }
        public override Result DoWork()
        {
            if (Checkers.ThisAppCanReadLocation())
            {
                LocationManager locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
                var loc = locationManager.GetLastKnownLocation(LocationManager.GpsProvider);

                var result = OpenWeatherMapClient.GetWeather(loc.Latitude.ToString(), loc.Longitude.ToString(), MeasurementUnits.Celsius, Locale.Default.Language)?.Result;
                if (result != null)
                {
                    return Result.InvokeSuccess();
                }
                else
                {
                    return Result.InvokeRetry();
                }
            }
            return Result.InvokeFailure();
        }
    }
}