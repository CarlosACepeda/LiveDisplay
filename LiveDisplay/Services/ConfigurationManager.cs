using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using AndroidX.Preference;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LiveDisplay.Services
{
    internal class ConfigurationManager
    {
        private readonly ISharedPreferences sharedPreferences;
        private readonly ISharedPreferencesEditor sharedPreferencesEditor;

        //Shared preferences.

        public ConfigurationManager()
        {
            sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
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
        public void SaveAValue(string key, Drawable value)
        {
            Bitmap bitmap = ((BitmapDrawable)value).Bitmap;

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            byte[] bitMapData = stream.ToArray();

            string actualValue= Android.Util.Base64.EncodeToString(bitMapData, Base64Flags.Default);

            sharedPreferencesEditor.PutString(key, actualValue);
            sharedPreferencesEditor.Commit();
        }

        public void SaveAValue(string key, float value)
        {
            sharedPreferencesEditor.PutFloat(key, value);
            sharedPreferencesEditor.Commit();
        }
        public void SaveValues(string key, ICollection<string> values)
        {
            sharedPreferencesEditor.PutStringSet(key, values);
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
        public Drawable RetrieveAValue(string key, bool dummyValue=true) 
        {
            var encodedValue= sharedPreferences.GetString(key, dummyValue.ToString());
            byte[] actualValue = Android.Util.Base64.Decode(encodedValue, Base64Flags.Default);

            MemoryStream memoryStream = new MemoryStream(actualValue);

            Drawable drawable = Drawable.CreateFromStream(memoryStream, string.Empty);
            return drawable;
        }
        public float RetrieveAValue(string key, float defaultIfNotFound=0)
        {
            return sharedPreferences.GetFloat(key, defaultIfNotFound);
        }
        public ICollection<string> RetrieveValues(string key)
        {
            ICollection<string> values= new Collection<string>();
            try
            {
                //GetStringSet doesn't work, apparently a Xamarin.Android bug.
                values = sharedPreferences.GetStringSet(key, null);
            }
            catch
            {
                var javaCollectionValues= (Android.Runtime.JavaCollection)sharedPreferences.All.Where(x=> x.Key == key).FirstOrDefault().Value;
                foreach (string item in javaCollectionValues)
                {
                    values.Add(item);
                }
            }
           return values;
        }
    }
}