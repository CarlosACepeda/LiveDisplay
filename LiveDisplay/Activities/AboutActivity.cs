using Android.App;
using Android.OS;
using Android.Support.V7.App;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/about")]
    public class AboutActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.About);

            // Create your application here
        }
    }
}