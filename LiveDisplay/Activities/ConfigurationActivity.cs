using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using System.Threading;

namespace LiveDisplay
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    internal class ConfigurationActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (NLChecker.IsNotificationListenerEnabled()==false)
            {
                //Haz algo
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Configuracion);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.my_toolbar);
            SetSupportActionBar(toolbar);

            SwitchCompat swEnableAwake = FindViewById<SwitchCompat>(Resource.Id.swEnableAwake);
            //O es para Objeto, e es para Evento.
            swEnableAwake.Click += (o, e) =>
            {
                if (swEnableAwake.Checked == true)
                {
                    //StartService(new Intent(this, typeof(AwakeService)));
                    Toast.MakeText(this, Resource.String.idk, ToastLength.Short).Show();
                }
                else if (swEnableAwake.Checked == false)
                {
                    Toast.MakeText(this, Resource.String.idk, ToastLength.Short).Show();
                    //StopService(new Intent(this, typeof(AwakeService)));
                }
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_config, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.preview:
                    {
                        Intent intent = new Intent(this, typeof(LockScreenActivity));
                        intent.AddFlags(ActivityFlags.SingleTop);
                        StartActivity(intent);
                        return true;
                    }

                case Resource.Id.notificationSettings:
                    {
                        string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
                        StartActivity(new Intent(lel).AddFlags(ActivityFlags.NewTask));
                        return true;
                    }
            }
            return true;
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}