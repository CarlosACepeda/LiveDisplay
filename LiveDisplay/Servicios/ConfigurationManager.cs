using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LiveDisplay.Servicios
{
    class ConfigurationManager
    {
        ISharedPreferences sharedPreferences;
        ISharedPreferencesEditor sharedPreferencesEditor;
        //Shared preferences.
        public ConfigurationManager(ISharedPreferences sharedPreferences)
        {
            sharedPreferencesEditor = sharedPreferences.Edit();
        }
        public void SaveAValue(string key, bool value)
        {
            sharedPreferencesEditor.PutBoolean(key, value);
            sharedPreferencesEditor.Commit();
        }
        public bool RetrieveAValue(string key, bool value)
        {
            return sharedPreferences.GetBoolean(key, value);
        }

    }
}