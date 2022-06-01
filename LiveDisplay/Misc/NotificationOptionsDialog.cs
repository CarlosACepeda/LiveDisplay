using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LiveDisplay.Models;
using LiveDisplay.Services;
using LiveDisplay.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDisplay.Misc
{
    internal class NotificationOptionsDialog: Dialog
    {
        public Context c;
        public OpenNotification openNotification;
        public Button action2, action3;
        float x; //dialog location
        int dialogWidth = 400;
        int dialogHeight = 350;
        int notificationViewWidth;
        public NotificationOptionsDialog(Context c, OpenNotification openNotification, float notificationX, int notificationViewWidth) : base(c, Resource.Style.LiveDisplayDialogTheme)
        {
            this.c = c;
            this.openNotification = openNotification;
            this.x = CalculateXDialogLocation(notificationX);
            this.notificationViewWidth = notificationViewWidth;
        }
        float CalculateXDialogLocation(float x)
        {
            if(IsNotificationLocatedAtRight(x))
            {
                this.x = x - dialogWidth - notificationViewWidth;
            }
            else
            {
                this.x = x + notificationViewWidth;
            }
            return this.x;
        }

        bool IsNotificationLocatedAtRight(float notification_x)
        {
            var displayMetrics = new Android.Util.DisplayMetrics();
            Window.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
            var screenwidth = displayMetrics.WidthPixels;
            return notification_x > (screenwidth / 2);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.notification_options_dialog);
            action2= FindViewById<Button>(Resource.Id.action2);
            action3= FindViewById<Button>(Resource.Id.action3);
            SetCanceledOnTouchOutside(true);
            SetCancelable(true);

            action2.Click += Action_Click;
            action3.Click += Action_Click;
            
        }

        private void Action_Click(object sender, EventArgs e)
        {
            var clickedButton = (Button)sender;
            var level= LevelsOfAppBlocking.None;
            switch (clickedButton.Id)
            {
                case Resource.Id.action2:
                    level = LevelsOfAppBlocking.Ignore;
                    NotificationHijackerWorker.RemoveNotification(openNotification, true);
                    break;
                case Resource.Id.action3:
                    level = LevelsOfAppBlocking.Remove;
                    NotificationHijackerWorker.RemoveNotification(openNotification);
                    break;
            }
            using (ConfigurationManager configurationManager = new ConfigurationManager(AppPreferences.Default))
            {
                configurationManager.SaveAValue(openNotification.ApplicationPackage, (int)level); 
            }
            Dismiss();
        }
        public override void Show()
        {
            // Setting dialogview
            Window.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.CenterVertical);

            //Window.SetLayout(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            Window.SetLayout(dialogWidth, dialogHeight);
            var param = Window.Attributes;
            param.X = (int)x;
            Window.Attributes = param;
            AddFlags();
            base.Show();
        }
        void AddFlags()
        {
            using (var view = Window.DecorView)
            {
                var uiOptions = (int)view.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                newUiOptions |= (int)SystemUiFlags.Immersive;
                // This option will make bars disappear by themselves
                newUiOptions |= (int)SystemUiFlags.ImmersiveSticky;
                view.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
            }
        }

    }
}