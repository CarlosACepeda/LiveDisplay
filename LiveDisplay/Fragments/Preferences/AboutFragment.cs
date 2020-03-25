using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using LiveDisplay.Activities;

namespace LiveDisplay.Fragments.Preferences
{
    public class AboutFragment : PreferenceFragmentCompat
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.about);
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.about, false);
            Preference githubprojectpreference = FindPreference("contributetoproject");
            githubprojectpreference.PreferenceClick += Githubprojectpreference_PreferenceClick;

        }
        private void Githubprojectpreference_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            string url = "https://github.com/CarlosACepeda/LiveDisplay/";
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse(url));
            StartActivity(intent);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}