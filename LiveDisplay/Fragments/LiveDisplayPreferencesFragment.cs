using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Factories;
using LiveDisplay.Misc;
using static Android.Preferences.Preference;

namespace LiveDisplay.Fragments
{
    public class LiveDisplayPreferencesFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        public override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.prefs);
            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);

        }
        public override void OnPause()
        {
            base.OnPause();
            sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1 && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri uri = data.Data;
                try
                {

                    BackgroundFactory background = new BackgroundFactory();
                    background.SaveImagePath(uri);
                    background = null;

                }
                catch
                {

                }
            }
        }
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key == ConfigurationParameters.changewallpaper)
            {
            //1 means 'User will pick custom wallpaper' 0 means use default.
            if (sharedPreferences.GetInt(ConfigurationParameters.changewallpaper, 0)==1)
            {
                using (Intent intent = new Intent())
                {
                    intent.SetType("image/*");
                    intent.SetAction(Intent.ActionGetContent);
                    //here 1 means the request code.
                    StartActivityForResult(Intent.CreateChooser(intent, "Pick image"), 1);
                }
                    
            }
            }
            
        }
        
            
        
    }
}