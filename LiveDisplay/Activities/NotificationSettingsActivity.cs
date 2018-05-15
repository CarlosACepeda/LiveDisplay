using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using LiveDisplay.Misc;
using System;

namespace LiveDisplay.Activities
{
    [Activity(Label = "@string/notificationsettings")]
    public class NotificationSettingsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private Android.Support.V7.Widget.CardView notificationOptionsPanel;
        private CheckBox cbxEnableNotificationListener;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.NotificationSettings);
            BindViews();
            BindEvents();
           
        }
        protected override void OnResume()
        {
            base.OnResume();
            if (NLChecker.IsNotificationListenerEnabled() == true)
            {
                cbxEnableNotificationListener.Checked = true;
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnbindViews();
            Finish();
        }

        private void UnbindViews()
        {
            cbxEnableNotificationListener.CheckedChange -= CbxEnableNotificationListener_CheckedChange;
        }

        private void BindViews()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.notificationSettingsToolbar);
            SetSupportActionBar(toolbar);
            notificationOptionsPanel = FindViewById<Android.Support.V7.Widget.CardView>(Resource.Id.notificationOptionsPanel);
            cbxEnableNotificationListener = FindViewById<CheckBox>(Resource.Id.cbxEnableNotificationListener);
        }
        private void BindEvents()
        {
            cbxEnableNotificationListener.CheckedChange += CbxEnableNotificationListener_CheckedChange;
        }

        private void CbxEnableNotificationListener_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (cbxEnableNotificationListener.Checked == true)
            {
                if (NLChecker.IsNotificationListenerEnabled() == true)
                {
                    notificationOptionsPanel.Visibility = ViewStates.Visible;
                }
                else
                {
                    //Prompt a message to go to NotificationListenerService.
                    cbxEnableNotificationListener.Checked = false;
                    Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                    builder.SetMessage(Resource.String.dialognldisabledmessage);
                    builder.SetPositiveButton(Resource.String.dialognldisabledpositivebutton, new EventHandler<DialogClickEventArgs>(OnDialogPositiveButtonEventArgs));
                    builder.SetNegativeButton(Resource.String.dialognldisablednegativebutton, new EventHandler<DialogClickEventArgs>(OnDialogNegativeButtonEventArgs));
                    builder.Show();
                }

            }
            else if (cbxEnableNotificationListener.Checked == false)
            {
                notificationOptionsPanel.Visibility = ViewStates.Gone;
                string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
                Intent intent = new Intent(lel).AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
                intent = null;
            }
        }
        private void OnDialogNegativeButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            //Nada.
        }
        private void OnDialogPositiveButtonEventArgs(object sender, DialogClickEventArgs e)
        {
            string lel = Android.Provider.Settings.ActionNotificationListenerSettings;
            Intent intent = new Intent(lel).AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            intent = null;
        }
    }
}