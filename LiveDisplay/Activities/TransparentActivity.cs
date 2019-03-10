using Android.App;
using Android.OS;

namespace LiveDisplay.Activities
{
    [Activity(Label = "TransparentActivity")]
    public class TransparentActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Finish();
        }
    }
}