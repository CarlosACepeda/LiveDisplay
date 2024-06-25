using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Preference;
using LiveDisplay.Services.Configuration;
using System;

namespace LiveDisplay.Services
{
    [Service(Label = "@string/app_name")]
    public class SharedPreferenceListenerService : Service, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private readonly ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        public static event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            Type type= null;
            object value= null;
            foreach(var sharedPreference in sharedPreferences.All)
            {
                if (sharedPreference.Key == key)
                {
                    value = sharedPreference.Value;
                    type = sharedPreference.Value.GetType();
                }
            }

            OnConfigurationChanged(new ConfigurationChangedEventArgs
            {
                Key = key,
                Value = value,
                ValueType = type
            });

            Console.WriteLine($"{key}: was CHANGED");
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Console.WriteLine("SHARED PREFERENCE LISTENER ACTIVE!");
            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

            return base.OnStartCommand(intent, flags, startId);
        }
        public override void OnDestroy()
        {
            sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
            Console.WriteLine("SHARED PREFERENCE LISTENER NOT ACTIVE!");
            base.OnDestroy();
        }
        void OnConfigurationChanged(ConfigurationChangedEventArgs e)
        {
            ConfigurationChanged?.Invoke(
                null, e);
        }
    }
}