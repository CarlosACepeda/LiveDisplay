using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Servicios.Wallpaper;
using System;

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
        private const string DecoratedCustomViewStyle = "android.app.Notification$DecoratedCustomViewStyle";

        private BuildVersionCodes currentAndroidVersion = Build.VERSION.SdkInt;
        private GravityFlags actionButtonsGravity;
        private GravityFlags actionButtonsContainerGravity;
        private bool actionTextsAreinCapitalLetters;
        private bool shouldShowIcons;
        private string actionTextsTypeface;
        private int actiontextMaxLines;
        private bool isCollapsed;

        private const int DefaultActionIdentificator = Resource.String.defaulttag;

        private LinearLayout notificationActions;
        private TextView title;
        private TextView text;
        private TextView applicationName;
        private TextView subtext;
        private TextView when;
        private ImageButton closenotificationbutton;
        private LinearLayout inlineNotificationContainer;
        private EditText inlineresponse;
        private ImageButton sendinlineresponse;
        private ImageButton togglenotificationcollapse;
        private ProgressBar notificationProgress;

        public static event EventHandler<bool> SendInlineResponseAvailabityChanged;

        private Resources resources;
        private AndroidX.Fragment.App.Fragment notificationFragment; //Required to do operations such as hiding the keyboard.
        private View notificationView;

        //TODO: It should allow apply style to multiple View types.
        //For example, actually it only applies the style to the NotificationWidget
        //But not the Floating Notification.
        public NotificationStyleApplier(ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
        {
            this.notificationView = notificationView;
            this.notificationFragment = notificationFragment;
            notificationActions = notificationView.FindViewById<LinearLayout>(Resource.Id.notificationActions);
            title = notificationView.FindViewById<TextView>(Resource.Id.tvTitulo);
            text = notificationView.FindViewById<TextView>(Resource.Id.tvTexto);
            applicationName = notificationView.FindViewById<TextView>(Resource.Id.tvAppName);
            subtext = notificationView.FindViewById<TextView>(Resource.Id.tvnotifSubtext);
            when = notificationView.FindViewById<TextView>(Resource.Id.tvWhen);
            closenotificationbutton = notificationView.FindViewById<ImageButton>(Resource.Id.closenotificationbutton);
            inlineNotificationContainer = notificationView.FindViewById<LinearLayout>(Resource.Id.inlineNotificationContainer);
            inlineresponse = notificationView.FindViewById<EditText>(Resource.Id.tvInlineText);
            sendinlineresponse = notificationView.FindViewById<ImageButton>(Resource.Id.sendInlineResponseButton);
            togglenotificationcollapse = notificationView.FindViewById<ImageButton>(Resource.Id.toggleCollapse);
            notificationProgress = notificationView.FindViewById<ProgressBar>(Resource.Id.notificationprogress);

            //This set's default options for actions and related.
            //Each notification Style can override these parameters.
            switch (currentAndroidVersion)
            {
                case BuildVersionCodes.Kitkat:
                case BuildVersionCodes.KitkatWatch:
                case BuildVersionCodes.Lollipop:
                case BuildVersionCodes.LollipopMr1:
                case BuildVersionCodes.M:
                    actionButtonsGravity = GravityFlags.Left | GravityFlags.CenterVertical;
                    actionButtonsContainerGravity = GravityFlags.Left;
                    actionTextsAreinCapitalLetters = false;
                    shouldShowIcons = true;
                    actionTextsTypeface = "sans-serif-condensed";
                    shouldShowIcons = false;
                    actiontextMaxLines = 1;
                    break;

                case BuildVersionCodes.N:
                case BuildVersionCodes.NMr1:
                case BuildVersionCodes.O:
                case BuildVersionCodes.OMr1:
                case BuildVersionCodes.P:
                case (BuildVersionCodes)29: //API 29, Android Q (Early support) TODO
                    actionButtonsGravity = GravityFlags.Left | GravityFlags.CenterVertical;
                    actionButtonsContainerGravity = GravityFlags.Left;
                    actionTextsAreinCapitalLetters = true;
                    shouldShowIcons = false;
                    actionTextsTypeface = "sans-serif-condensed";
                    shouldShowIcons = false; //MediaStyle overrides this.
                    actiontextMaxLines = 1;
                    break;

                default:
                    break;
            }

            closenotificationbutton.Click += Closenotificationbutton_Click;
            togglenotificationcollapse.Click += Togglenotificationcollapse_Click;
        }

        public NotificationStyleApplier(IStyle style)
        {

        }

        public void ApplyStyle(OpenNotification notification)
        {
            //The progress thing does not require a Style to be applied.
            if (notification.GetProgressMax() > 0)
            {
                notificationProgress.Visibility = ViewStates.Visible;
                if (notification.IsProgressIndeterminate())
                {
                    notificationProgress.Indeterminate = true;
                }
                else
                {
                    notificationProgress.Max = notification.GetProgressMax();
                    notificationProgress.Progress = notification.GetProgress();
                }
            }
            else
            {
                notificationProgress.Visibility = ViewStates.Gone;
            }

            title.Text = notification.Title();
            text.Text = notification.Text();
            applicationName.Text = notification.AppName();
            subtext.Text = notification.SubText();
            when.Text = notification.When();
            closenotificationbutton.SetTag(DefaultActionIdentificator, notification);
            closenotificationbutton.Visibility = notification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;
            notificationActions.Visibility = notification.HasActions() ? ViewStates.Visible : ViewStates.Gone;
            inlineNotificationContainer.Visibility = ViewStates.Invisible;

            switch (notification.Style())
            {
                case BigPictureStyle:

                    var notificationBigPicture = new BitmapDrawable(notification.BigPicture());
                    WallpaperPublisher.ChangeWallpaper(new WallpaperChangedEventArgs
                    {
                        BlurLevel = 0,
                        OpacityLevel = 125,
                        SecondsOfAttention = 5,
                        Wallpaper = notificationBigPicture,
                        WallpaperPoster = WallpaperPoster.Notification,
                    });
                    break;

                case InboxStyle:
                    text.SetMaxLines(6); //Should be configurable(?)
                    break;

                case BigTextStyle:

                    text.SetMaxLines(9); //Shoud be configurable(?)
                    text.Text = notification.GetBigText();
                    ApplyActionsStyle(notification);
                    break;

                case MediaStyle:
                    when.Text = string.Empty; //The MediaStyle shouldn't show a timestamp.
                    //notification.StartMediaCallback();
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
                    //Theres still things to do, this style has several stuff in Pie and Q versions.
                    //TODO
                    break;

                case DecoratedCustomViewStyle:
                    //todo
                    break;

                default:
                    //Do nothing, yet.
                    break;
            }
            ApplyActionsStyle(notification);
        }

        private void Togglenotificationcollapse_Click(object sender, EventArgs e)
        {
            //TODO
        }

        private void Closenotificationbutton_Click(object sender, EventArgs e)
        {
            ImageButton closenotificationbutton = sender as ImageButton;
            OpenNotification openNotification = closenotificationbutton.GetTag(DefaultActionIdentificator) as OpenNotification;
            openNotification.Cancel();
            notificationView.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            notificationView.Visibility = ViewStates.Invisible;
        }

        private void AnActionButton_Click(object sender, System.EventArgs e)
        {
            Button actionButton = sender as Button;
            OpenAction openAction = actionButton.GetTag(DefaultActionIdentificator) as OpenAction;

            if (openAction.ActionRepresentDirectReply())
            {
                if (new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.EnableQuickReply))
                {
                    notificationActions.Visibility = ViewStates.Invisible;
                    inlineNotificationContainer.Visibility = ViewStates.Visible;
                    inlineresponse.Hint = openAction.GetPlaceholderTextForInlineResponse();
                    sendinlineresponse.SetTag(DefaultActionIdentificator, openAction);
                    sendinlineresponse.Click += Sendinlineresponse_Click;

                    SendInlineResponseAvailabityChanged?.Invoke(null, true); //Is currently showing.
                }
            }
            else
            {
                openAction.ClickAction();
                SendInlineResponseAvailabityChanged?.Invoke(null, false); //Here I assume the send inline textbox is not present, because the action simply does not represent a direct reply.
            }
        }

        private void Sendinlineresponse_Click(object sender, EventArgs e)
        {
            ImageButton actionButton = sender as ImageButton;
            OpenAction openAction = actionButton.GetTag(DefaultActionIdentificator) as OpenAction;
            openAction.SendInlineResponse(inlineresponse.Text);
            inlineresponse.Text = string.Empty;
            notificationActions.Visibility = ViewStates.Visible;
            inlineNotificationContainer.Visibility = ViewStates.Invisible;
            // Check if no view has focus:
            View view = notificationFragment.Activity.CurrentFocus;
            if (view != null)
            {
                InputMethodManager imm = (InputMethodManager)notificationFragment.Activity.GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromInputMethod(view.WindowToken, 0);
            }
            SendInlineResponseAvailabityChanged?.Invoke(null, false); //Operation finished, inline response text is not available.
        }

        public void ApplyActionsStyle(OpenNotification notification)
        {
            notificationActions?.RemoveAllViews();
            if (notification.HasActions())
            {
                var actions = notification.RetrieveActions();
                foreach (Notification.Action action in actions)
                {
                    OpenAction openAction = new OpenAction(action);
                    Button actionButton = new Button(Application.Context);
                    float weight = 1f / actions.Count;
                    actionButton.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight);
                    actionButton.SetTextColor(Color.White); //Should change in MediaStyle (?)
                    actionButton.SetTag(DefaultActionIdentificator, openAction);
                    actionButton.Click += AnActionButton_Click;
                    actionButton.Gravity = actionButtonsGravity;
                    actionButton.SetMaxLines(actiontextMaxLines);
                    TypedValue outValue = new TypedValue();
                    Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackground, outValue, true);
                    actionButton.SetBackgroundResource(outValue.ResourceId);
                    actionButton.SetTypeface(Typeface.Create(actionTextsTypeface, TypefaceStyle.Normal), TypefaceStyle.Normal);
                    //notificationActions.SetGravity(actionButtonsContainerGravity);

                    Log.Info("LiveDisplay", openAction.Title());

                    if (notification.Style() != MediaStyle)
                        actionButton.Text = openAction.Title();

                    if (actionTextsAreinCapitalLetters == false)
                    {
                        actionButton.TransformationMethod = null; //Disables all caps text.
                    }

                    Handler looper = new Handler(Looper.MainLooper);
                    looper.Post(() =>
                        {
                            if (shouldShowIcons || notification.Style() == MediaStyle) //The MediaStyle allows icons to be shown.
                            {
                                actionButton.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(), null, null, null);
                            }

                            notificationActions.AddView(actionButton);
                        });
                }
            }
        }

        public void ToggleExtendedActions(bool extended)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}