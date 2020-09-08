using Android.App;
using Android.Content;
using System;

namespace LiveDisplay.Servicios
{
    internal class ConfigurationManager : Java.Lang.Object
    {
        private ISharedPreferences sharedPreferences;
        private ISharedPreferencesEditor sharedPreferencesEditor;

        //Shared preferences.
        public ConfigurationManager(AppPreferences preferences)
        {
            switch (preferences)
            {
                case AppPreferences.Default:
                    sharedPreferences = AndroidX.Preference.PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                    break;

                case AppPreferences.Weather:
                    sharedPreferences = Application.Context.GetSharedPreferences("weatherpreferences", FileCreationMode.Private);
                    break;
            }
            if (sharedPreferences == null) throw new InvalidOperationException("Shared preferences can't be null!");
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

        public void SaveAValue(string key, int value)
        {
            sharedPreferencesEditor.PutInt(key, value);
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

    public enum AppPreferences
    {
        Default = 1,
        Weather = 2
    }
}