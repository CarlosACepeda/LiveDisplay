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
    internal class ConfigurationManager: Java.Lang.Object
    {
        private ISharedPreferences sharedPreferences;
        private ISharedPreferencesEditor sharedPreferencesEditor;

        //Shared preferences.
        public ConfigurationManager(ISharedPreferences sharedPreferences)
        {
            this.sharedPreferences = sharedPreferences;
            sharedPreferencesEditor = sharedPreferences.Edit();
        }
        public void SaveAValue(string key, bool value)
        {
            sharedPreferencesEditor.PutBoolean(key, value);
            sharedPreferencesEditor.Commit();
        }
        public void SaveAValue(string key, string value)
        {
            sharedPreferencesEditor.PutString(key, value);
            sharedPreferencesEditor.Commit();
        }
        public bool RetrieveAValue(string key)
        {
            return sharedPreferences.GetBoolean(key, false);
        }
        public string RetrieveAValue(string key, string defValue)
        {
            return sharedPreferences.GetString(key, defValue);
        }
        public int RetrieveAValue(string key, int defValue)
        {
            return sharedPreferences.GetInt(key, defValue);
        }


    }
}