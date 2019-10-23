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
using System.Collections.Generic;
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
        private const string MessagingStyle = "android.app.Notification$MessagingStyle"; //Only available on API Level 24 and up.
        private const string BigTextStyle = "android.app.Notification$BigTextStyle";


        private LinearLayout notificationActions;
        private TextView titulo;
        private TextView texto;
        private TextView appName;
        private TextView subtext;
        private TextView when;
        private ImageButton closenotificationbutton;
        private LinearLayout inlineNotificationContainer;
        private EditText inlineresponse;
        private Button sendinlineresponse;


        private Resources resources;
        private View notificationView;

        public NotificationStyleApplier(ref LinearLayout notificationView)
        {
            this.notificationView = notificationView;
            notificationActions= notificationView.FindViewById<LinearLayout>(Resource.Id.notificationActions);
            titulo = notificationView.FindViewById<TextView>(Resource.Id.tvTitulo);
            texto = notificationView.FindViewById<TextView>(Resource.Id.tvTexto);
            appName = notificationView.FindViewById<TextView>(Resource.Id.tvAppName);
            subtext = notificationView.FindViewById<TextView>(Resource.Id.tvnotifSubtext);
            when = notificationView.FindViewById<TextView>(Resource.Id.tvWhen);
            closenotificationbutton = notificationView.FindViewById<ImageButton>(Resource.Id.closenotificationbutton);            
            inlineNotificationContainer = notificationView.FindViewById<LinearLayout>(Resource.Id.inlineNotificationContainer);

            closenotificationbutton.Click += Closenotificationbutton_Click;


        }

        public void ApplyStyle(OpenNotification notification)
        {
            List<Notification.Action> actions = new List<Notification.Action>();
            if (notification.HasActionButtons() == true)
            {
                actions = notification.RetrieveActions();
            }
            switch (notification.Style())
            {
                case BigPictureStyle:

                    titulo.Text = notification.Title();
                    texto.Text = notification.Text();
                    appName.Text = notification.AppName();
                    subtext.Text = notification.SubText();
                    when.Text = notification.When();
                    closenotificationbutton.Visibility = notification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;

                        var notificationBigPicture = new BitmapDrawable(notification.BigPicture());
                        WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                        {
                            BlurLevel=0,
                            OpacityLevel= 125,
                            SecondsOfAttention= 5,
                            Wallpaper= notificationBigPicture,
                            WallpaperPoster= WallpaperPoster.Notification,
                        });
                    notificationActions.RemoveAllViews();

                    if (notification.HasActionButtons() == true)
                    {
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
                            Handler looper = new Handler(Looper.MainLooper);
                            looper.Post(() =>
                            {
                                anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                                notificationActions.AddView(anActionButton);
                            });

                            break;
                        }
                    }
                    break;

                case InboxStyle:
                    titulo.Text = notification.Title();
                    texto.SetMaxLines(6);
                    texto.Text = notification.GetTextLines();
                    appName.Text = notification.AppName();
                    subtext.Text = notification.SubText();
                    when.Text = notification.When();
                    closenotificationbutton.Visibility = notification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;
                    notificationActions.RemoveAllViews();
                    if (notification.HasActionButtons() == true)
                    {
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
                            Handler looper = new Handler(Looper.MainLooper);
                            looper.Post(() =>
                            {
                                anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                                notificationActions.AddView(anActionButton);
                            });

                            break;
                        }
                    }
                    break;

                case BigTextStyle:

                    titulo.Text = notification.Title();
                    texto.SetMaxLines(9);
                    texto.Text = notification.GetBigText();
                    appName.Text = notification.AppName();
                    subtext.Text = notification.SubText();
                    when.Text = notification.When();
                    closenotificationbutton.Visibility = notification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;
                    notificationActions.RemoveAllViews();
                    if (notification.HasActionButtons() == true)
                    {
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
                            Handler looper = new Handler(Looper.MainLooper);
                            looper.Post(() =>
                            {
                                anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                                notificationActions.AddView(anActionButton);
                            });

                            break;
                        }
                    }
                    break;
                case MediaStyle:

                    titulo.Text = notification.Title();
                    texto.Text = notification.Text();
                    appName.Text = notification.AppName();
                    subtext.Text = notification.SubText();
                    when.Text = string.Empty; //The MediaStyle shouldn't show a timestamp.
                    closenotificationbutton.Visibility = notification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;

                    notificationActions.RemoveAllViews();

                    //in the media style, grab the action buttons, remove the text and load images instead
                    if (notification.HasActionButtons() == true)
                    {
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
                            notificationActions.AddView(anActionButton);
                        };
                    }
                    var notificationMediaArtwork = new BitmapDrawable(notification.MediaArtwork());
                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        BlurLevel = 0,
                        OpacityLevel = 125,
                        SecondsOfAttention = 5,
                        Wallpaper = notificationMediaArtwork,
                        WallpaperPoster = WallpaperPoster.Notification,
                    });

                    break;

                case MessagingStyle:
                    //Only available in API Level 24 and Up.
                    

                    titulo.Text = notification.Title();
                    texto.Text = notification.Text();
                    appName.Text = notification.AppName();
                    subtext.Text = notification.SubText();
                    when.Text = notification.When();
                    closenotificationbutton.Visibility = notification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;
                    notificationActions.RemoveAllViews();

                    if (notification.HasActionButtons() == true)
                    {

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
                            Handler looper = new Handler(Looper.MainLooper);
                            looper.Post(() =>
                            {
                                anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                                notificationActions.AddView(anActionButton);
                            });

                            break;
                        }
                    }

                    break;

                default:
                    ApplyDefault(notification);
                    break;
            }
        }

        private void AnActionButton_Click(object sender, System.EventArgs e)
        {
            
        }

        private void ApplyDefault(OpenNotification notification)
        {
            titulo.Text = notification.Title();
            texto.Text = notification.Text();
            appName.Text = notification.AppName();
            subtext.Text = notification.SubText();
            when.Text = notification.When();
            closenotificationbutton.Visibility = notification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;



            var actionsViews = notificationView.FindViewById<LinearLayout>(Resource.Id.notificationActions);
            actionsViews.RemoveAllViews();

            if (notification.HasActionButtons() == true)
            {
                var actions = notification.RetrieveActions();
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
                    Handler looper = new Handler(Looper.MainLooper);
                    looper.Post(() =>
                    {
                        anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                        actionsViews.AddView(anActionButton);
                    });
                }
            }
        }

        private void Closenotificationbutton_Click(object sender, EventArgs e)
        {
            
            //if (notifi.IsRemovable())
            //{
            //    using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
            //    {
            //        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            //        {
            //            int notiId = CatcherHelper.statusBarNotifications[position].Id;
            //            string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
            //            string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
            //            slave.CancelNotification(notiPack, notiTag, notiId);
            //        }
            //        else
            //        {
            //            slave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
            //        }
            //    }
            //    notificationView.Visibility = ViewStates.Invisible;
            //    titulo.Text = null;
            //    texto.Text = null;
            //    notificationActions.RemoveAllViews();
            //}                //if (notifi.IsRemovable())
            //{
            //    using (NotificationSlave slave = NotificationSlave.NotificationSlaveInstance())
            //    {
            //        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            //        {
            //            int notiId = CatcherHelper.statusBarNotifications[position].Id;
            //            string notiTag = CatcherHelper.statusBarNotifications[position].Tag;
            //            string notiPack = CatcherHelper.statusBarNotifications[position].PackageName;
            //            slave.CancelNotification(notiPack, notiTag, notiId);
            //        }
            //        else
            //        {
            //            slave.CancelNotification(CatcherHelper.statusBarNotifications[position].Key);
            //        }
            //    }
            //    notificationView.Visibility = ViewStates.Invisible;
            //    titulo.Text = null;
            //    texto.Text = null;
            //    notificationActions.RemoveAllViews();
            //}
        }        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}