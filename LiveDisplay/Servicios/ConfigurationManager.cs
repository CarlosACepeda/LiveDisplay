using Android.Content;

namespace LiveDisplay.Servicios
{
    internal class ConfigurationManager
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
        public bool RetrieveAValue(string key)
        {           
            return sharedPreferences.GetBoolean(key, false);
        }
            
    }
}