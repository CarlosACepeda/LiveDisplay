using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios.Wallpaper;
using System;
using System.Threading;

namespace LiveDisplay.Servicios.Notificaciones.NotificationStyle
{
    /// <summary>
    /// Self explaining, this will apply the notification styles.
    /// But, I will interpret them differently as android does, to be adapted to my lockscreen.
    /// </summary>
    internal class NotificationStyleApplier : Java.Lang.Object
    {
        private const string BigPictureStyle = "android.app.Notification$BigPictureStyle";
        private const string InboxStyle = "android.app.Notification$InboxStyle";
        private const string MediaStyle = "android.app.Notification$MediaStyle";
        private const string MessagingStyle = "android.app.Notification$MessagingStyle";
        private Resources resources;
        private OpenNotification openNotification;
        private View notificationView;

        public NotificationStyleApplier(ref LinearLayout notificationView, OpenNotification openNotification)
        {
            this.notificationView = notificationView;
            this.openNotification = openNotification;
        }

        public void ApplyStyle(string which)
        {
            switch (which)
            {
                case BigPictureStyle:
                    //Idk how to implement this yet.

                    //ThreadPool.QueueUserWorkItem(method =>
                    //{
                    //    var notificationBigPicture = new BitmapDrawable(openNotification.BigPicture());
                    //using (var h = new Handler(Looper.MainLooper)) //Using UI Thread
                    //    h.Post(() =>
                    //    {
                    //        if (notificationView != null)
                    //            notificationView.Background = notificationBigPicture;
                    //    });
                    //});
                    ApplyDefault();
                    break;

                case InboxStyle:
                    break;

                case MediaStyle:

                    //in the media style, grab the action buttons, remove the text and load images instead
                    var actionsViews= notificationView.FindViewById<LinearLayout>(Resource.Id.notificationActions);
                    if (openNotification.HasActionButtons() == true)
                    {
                        var actions = openNotification.RetrieveActions();
                        foreach (var a in actions)
                        {
                            OpenAction openAction = new OpenAction(a);
                            float weight = (float)1 / actions.Count;

                            Button anActionButton = new Button(Application.Context)
                            {
                                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                            };
                            anActionButton.Click += (o, eventargs) =>
                            {
                                openAction.ClickAction();
                            };
                            anActionButton.Gravity = GravityFlags.CenterVertical;
                            TypedValue outValue = new TypedValue();
                            Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                            anActionButton.SetBackgroundResource(outValue.ResourceId);
                            anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                            actionsViews.AddView(anActionButton);
                        };
                    }

                    break;

                case MessagingStyle:
                    break;
                default:
                    ApplyDefault();
                    break;
            }
        }

        private void ApplyDefault()
        {
            var actionsViews = notificationView.FindViewById<LinearLayout>(Resource.Id.notificationActions);
            if (openNotification.HasActionButtons() == true)
            {
                var actions = openNotification.RetrieveActions();
                foreach (var a in actions)
                {
                    OpenAction openAction = new OpenAction(a);
                    float weight = (float)1 / actions.Count;

                    Button anActionButton = new Button(Application.Context)
                    {
                        LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight),
                        Text = openAction.GetTitle()
                    };
                    anActionButton.TransformationMethod = null;
                    anActionButton.SetTypeface(Typeface.Create("sans-serif-condensed", TypefaceStyle.Normal), TypefaceStyle.Normal);
                    anActionButton.SetMaxLines(1);
                    anActionButton.SetTextColor(Color.White);
                    anActionButton.Click += (o, eventargs) =>
                    {
                        openAction.ClickAction();
                    };
                    anActionButton.Gravity = GravityFlags.CenterVertical;
                    TypedValue outValue = new TypedValue();
                    Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackgroundBorderless, outValue, true);
                    anActionButton.SetBackgroundResource(outValue.ResourceId);
                    //anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                    actionsViews.AddView(anActionButton);

                }
                
            }

        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}