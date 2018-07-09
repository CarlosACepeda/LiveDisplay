using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Fragments
{
    public class LiveDisplayPreferencesFragment : PreferenceFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.prefs);
        }
    }
}