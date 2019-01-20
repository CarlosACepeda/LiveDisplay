using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios;

namespace LiveDisplay.Activities
{
    [Activity(Label = "Weather", Theme = "@style/LiveDisplayThemeDark.NoActionBar")]
    public class WeatherSettingsActivity : AppCompatActivity
    {
        ConfigurationManager configurationManager;
        ISharedPreferences sharedPreferences;
        private Android.Support.V7.Widget.Toolbar toolbar;
        private EditText city;
        private Switch useimperialsystem;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            sharedPreferences= Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private);


            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.WeatherSettings);
            using (toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar))
            {
                SetSupportActionBar(toolbar);
            }
            city = FindViewById<EditText>(Resource.Id.city);
            useimperialsystem = FindViewById<Switch>(Resource.Id.useimperialsystem);
            city.FocusChange += City_FocusChange;
            useimperialsystem.CheckedChange += Useimperialsystem_CheckedChange;
            LoadConfiguration();
        }

        private void Useimperialsystem_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            switch (e.IsChecked)
            {
                case true:
                    configurationManager.SaveAValue(ConfigurationParameters.UseImperialSystem, true);
                    break;
                case false:
                    configurationManager.SaveAValue(ConfigurationParameters.UseImperialSystem, false);
                    break;
                default:
                    break;
            }
        }

        private void City_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus == false)
            {
                configurationManager.SaveAValue(ConfigurationParameters.City, city.Text);
            }
        }

        void LoadConfiguration()
        {
            using (configurationManager = new ConfigurationManager(sharedPreferences))
            {
                city.Text=
                configurationManager.RetrieveAValue(ConfigurationParameters.City, "");
                useimperialsystem.Checked =
                    configurationManager.RetrieveAValue(ConfigurationParameters.UseImperialSystem);
                
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            configurationManager.SaveAValue(ConfigurationParameters.City, city.Text); //Save before exit, because it might be possible that the EditText never loses focus.
           
            city.FocusChange -= City_FocusChange;
            useimperialsystem.CheckedChange -= Useimperialsystem_CheckedChange;
            city.Dispose();
            useimperialsystem.Dispose();
            configurationManager.Dispose();

        }
    }
}