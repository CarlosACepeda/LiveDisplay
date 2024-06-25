using Android.Content;
using Android.Util;
using AndroidX.Preference;

namespace LiveDisplay.Misc
{
    public class IntegerListPreference : ListPreference
    {
        public IntegerListPreference(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected override bool PersistString(string value)
        {
            int intValue = int.Parse(value);
            return PersistInt(intValue);
        }
        protected override string GetPersistedString(string defaultReturnValue)
        {
            int intValue;

            if (defaultReturnValue != null)
            {
                int intDefaultReturnValue = int.Parse(defaultReturnValue);
                intValue = GetPersistedInt(intDefaultReturnValue);
            }
            else
            {
                // We haven't been given a default return value, but we need to specify one when retrieving the value

                int persistedIntZero = GetPersistedInt(0);
                int persistedIntOne = GetPersistedInt(1);
                if (persistedIntOne== persistedIntZero)
                {
                    // The default value is being ignored, so we're good to go
                    intValue = GetPersistedInt(0);
                }
                else
                {
                    return GetPersistedInt(0).ToString();
                }
            }

            return intValue.ToString();
        }
    }

}