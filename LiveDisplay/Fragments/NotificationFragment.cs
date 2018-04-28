using Android.App;
using Android.OS;
using Android.Views;

namespace LiveDisplay.Fragments
{
    public class NotificationFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.NotificiationFrag, container, false);
            return v;
        }
    }
}