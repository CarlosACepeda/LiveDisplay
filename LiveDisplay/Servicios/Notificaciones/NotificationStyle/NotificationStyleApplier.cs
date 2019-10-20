using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using LiveDisplay.Servicios.Wallpaper;
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
        private EditText inlineresponse;
        private Button sendinlineresponse;

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
                        var notificationBigPicture = new BitmapDrawable(openNotification.BigPicture());
                        WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                        {
                            BlurLevel=0,
                            OpacityLevel= 125,
                            SecondsOfAttention= 5,
                            Wallpaper= notificationBigPicture,
                            WallpaperPoster= WallpaperPoster.Notification,
                        });

                    break;

                case InboxStyle:
                    //In the inbox style also populate a view Called TextLines(non existent yet) to fill it with a series of TextLines supplied to that notification.
                    break;

                case MediaStyle:

                    //in the media style, grab the action buttons, remove the text and load images instead
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
                    var notificationMediaArtwork = new BitmapDrawable(openNotification.MediaArtwork());
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
                    //I think it'll be useless anyway, as for implementing the Messaging Style the user should be capable of answering to the messages from the notification itself.
                    //And for now that's impossible.
                    break;

                default:
                    ApplyDefault();
                    break;
            }
        }

        private void AnActionButton_Click(object sender, System.EventArgs e)
        {
            
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
                    Handler looper = new Handler(Looper.MainLooper);
                    looper.Post(() =>
                    {
                        anActionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                        actionsViews.AddView(anActionButton);
                    });
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}