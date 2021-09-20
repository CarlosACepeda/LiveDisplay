using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using LiveDisplay.Misc;
using LiveDisplay.Services.Widget;
using System;

namespace LiveDisplay.Services.Notifications.NotificationStyle
{
    //Provides a basic way to show a notification on the lockscreen.
    public abstract class NotificationStyle
    {

        protected const string MediaStyle = "android.app.Notification$MediaStyle";

        public static event EventHandler<bool> SendInlineResponseAvailabityChanged;

        protected const int DefaultActionIdentificator = Resource.String.defaulttag;
        protected LinearLayout NotificationView { get; private set; }
        public AndroidX.Fragment.App.Fragment NotificationFragment { get; private set; }
        protected LinearLayout NotificationActions { get; private set; }
        protected TextView Title { get; private set; }
        protected TextView Text { get; private set; }
        protected TextView ApplicationName { get; private set; }
        protected TextView When { get; private set; }
        protected TextView Subtext { get; private set; }
        protected ProgressBar NotificationProgress { get; private set; }
        protected ImageButton CloseNotification { get; private set; }
        protected LinearLayout InlineResponseNotificationContainer { get; private set; }
        //protected TextView PreviousMessages { get; private set; }
        protected ImageButton Collapse { get; private set; }
        protected EditText InlineResponse { get; private set; }
        protected ImageButton SendInlineResponse { get; private set; }
        protected OpenNotification OpenNotification { get; private set; }
        protected Resources Resources { get; private set; }

        public NotificationStyle(OpenNotification openNotification, ref LinearLayout notificationView, AndroidX.Fragment.App.Fragment notificationFragment)
        {
            OpenNotification = openNotification;
            NotificationView = notificationView;
            NotificationFragment = notificationFragment;
            Resources = NotificationFragment.Resources;
            NotificationActions = FindView<LinearLayout>(Resource.Id.actions);
            Title = FindView<TextView>(Resource.Id.notification_title);
            Text = FindView<TextView>(Resource.Id.notification_text);
            ApplicationName = FindView<TextView>(Resource.Id.app_name);
            When = FindView<TextView>(Resource.Id.when);
            Subtext = FindView<TextView>(Resource.Id.subtext);
            NotificationProgress = FindView<ProgressBar>(Resource.Id.progress);
            CloseNotification = FindView<ImageButton>(Resource.Id.close_notification);
            InlineResponseNotificationContainer = FindView<LinearLayout>(Resource.Id.inline_notification_container);
            //PreviousMessages = FindView<TextView>(Resource.Id.previousMessages);
            Collapse = FindView<ImageButton>(Resource.Id.toggle_collapse);
            InlineResponse = FindView<EditText>(Resource.Id.inline_text);
            SendInlineResponse = FindView<ImageButton>(Resource.Id.send_response);
            CloseNotification.Click += CloseNotification_Click;
            Collapse.Click += Collapse_Click;
        }

        protected virtual void Collapse_Click(object sender, EventArgs e)
        {
            //Do nothing, it depends on actual implementations.
        }

        protected virtual void CloseNotification_Click(object sender, EventArgs e)
        {
            ImageButton closenotificationbutton = sender as ImageButton;
            OpenNotification openNotification = closenotificationbutton.GetTag(DefaultActionIdentificator) as OpenNotification;
            openNotification?.Cancel();
            NotificationView.SetTag(Resource.String.defaulttag, openNotification.GetCustomId());
            WidgetStatusPublisher.GetInstance().SetWidgetVisibility(new ShowParameters { Show = false, WidgetName = Constants.NOTIFICATION_FRAGMENT });
            NotificationView.Visibility = ViewStates.Invisible;
        }

        public virtual void ApplyStyle()
        {
            SetWhen();
            SetTitle();
            SetText();
            SetSubtext();
            SetTextMaxLines();
            SetApplicationName();
            SetProgress();
            SetCloseButtonVisibility();
            SetCloseButtonTag();
            SetNotificationActionsVisibility();
            SetNotificationInlineRespVisibility();
            SetNotificationPreviousMessageVisibility();
            ApplyActionsStyle();
        }

        protected virtual void SetTitle()
        {
            Title.Text = OpenNotification.Title;
        }

        protected virtual void SetText()
        {
            Text.Text = OpenNotification.Text;
        }

        protected virtual void SetTextMaxLines()
        {
            Text.SetMaxLines(2);
        }

        protected virtual void SetApplicationName()
        {
            ApplicationName.Text = OpenNotification.ApplicationOwnerName;
        }

        protected virtual void SetWhen()
        {
            When.Text = OpenNotification.When;
        }

        protected virtual void SetProgress()
        {
            if (OpenNotification.MaximumProgress > 0)
            {
                NotificationProgress.Visibility = ViewStates.Visible;
                NotificationProgress.Indeterminate = OpenNotification.ProgressIndeterminate;
                NotificationProgress.Max = OpenNotification.MaximumProgress;
                NotificationProgress.Progress = OpenNotification.Progress;
            }
            else
            {
                NotificationProgress.Visibility = ViewStates.Gone;
            }
        }

        protected virtual void SetSubtext()
        {
            Subtext.Text = OpenNotification.SubText;
        }

        protected virtual void SetCloseButtonVisibility()
        {
            CloseNotification.Visibility = OpenNotification.IsRemovable() ? ViewStates.Visible : ViewStates.Invisible;
        }

        protected virtual void SetCloseButtonTag()
        {
            CloseNotification.SetTag(DefaultActionIdentificator, OpenNotification);
        }

        protected virtual void SetNotificationActionsVisibility()
        {
            NotificationActions.Visibility = OpenNotification.HasActions() ? ViewStates.Visible : ViewStates.Invisible;
        }

        protected virtual void SetNotificationInlineRespVisibility()
        {
            InlineResponseNotificationContainer.Visibility = ViewStates.Invisible;
        }

        protected virtual void SetNotificationPreviousMessageVisibility()
        {
            //PreviousMessages.Text = string.Empty; //If a Messaging Style notification sets text here, then we clear it here when another instance of NotificationStyle is created.
            //PreviousMessages.Visibility = ViewStates.Gone;
        }

        public virtual void ApplyActionsStyle()
        {
            NotificationActions?.RemoveAllViews();
            if (OpenNotification.HasActions())
            {
                var actions = OpenNotification.Actions;
                for (int i = 0; i <= actions.Count - 1; i++)
                {
                    var action = actions[i];
                    OpenAction openAction = new OpenAction(action);
                    Button actionButton = new Button(Application.Context);
                    float weight = 1f / actions.Count;
                    actionButton.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.MatchParent, weight);
                    SetActionTextColor(actionButton);
                    SetActionTag(actionButton, openAction);
                    actionButton.Click += ActionButton_Click;
                    SetActionButtonGravity(actionButton);
                    SetActionButtonTextMaxLines(actionButton);
                    SetActionButtonTextTypeface(actionButton);
                    SetActionButtonBackground(actionButton);
                    SetActionText(actionButton, openAction.Title());
                    SetActionTextLowercase(actionButton);
                    SetActionIcon(actionButton, openAction);
                    AddActionToActionsView(actionButton, i);
                }
            }
        }

        protected virtual void SetActionTextColor(Button action)
        {
            action.SetTextColor(Android.Graphics.Color.White);
        }

        protected virtual void SetActionTag(Button action, OpenAction openAction)
        {
            action.SetTag(DefaultActionIdentificator, openAction);
        }

        protected virtual void ActionButton_Click(object sender, EventArgs e)
        {
            Button actionButton = sender as Button;
            OpenAction openAction = actionButton.GetTag(DefaultActionIdentificator) as OpenAction;
            if (openAction.ActionRepresentDirectReply())
            {
                if (new ConfigurationManager(AppPreferences.Default).RetrieveAValue(ConfigurationParameters.EnableQuickReply))
                {
                    NotificationActions.Visibility = ViewStates.Invisible;
                    InlineResponseNotificationContainer.Visibility = ViewStates.Visible;
                    InlineResponse.Hint = openAction.GetPlaceholderTextForInlineResponse();
                    SendInlineResponse.SetTag(DefaultActionIdentificator, openAction);
                    SendInlineResponse.Click += SendInlineResponse_Click;
                    SendInlineResponseAvailabityChanged?.Invoke(null, true);
                }
            }
            else
            {
                openAction.ClickAction();
            }
        }

        protected virtual void SetActionButtonGravity(Button action)
        {
            action.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
        }

        protected virtual void SetActionButtonTextMaxLines(Button action)
        {
            action.SetMaxLines(1);
        }

        protected virtual void SetActionButtonTextTypeface(Button action)
        {
            string typeface = "sans-serif-condensed";
            action.SetTypeface(Typeface.Create(typeface, TypefaceStyle.Normal), TypefaceStyle.Normal);
        }

        protected virtual void SetActionButtonBackground(Button action)
        {
            TypedValue outValue = new TypedValue();
            Application.Context.Theme.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackground, outValue, true);
            action.SetBackgroundResource(outValue.ResourceId);
        }

        protected virtual void SetActionText(Button action, string text)
        {
            action.Text = text;
        }

        protected virtual void SetActionTextLowercase(Button action)
        {
            action.TransformationMethod = null;
        }

        protected virtual void SetActionIcon(Button action, OpenAction openAction)
        {
            //In versions beyond Nougat the only notification that has icons is the MediaStyle one.
            if (Build.VERSION.SdkInt<= BuildVersionCodes.N  || OpenNotification.Style == MediaStyle)
                action.SetCompoundDrawablesRelativeWithIntrinsicBounds(openAction.GetActionIcon(Color.White), null, null, null);
        }

        protected void AddActionToActionsView(Button button, int position)
        {
            Handler looper = new Handler(Looper.MainLooper);
            looper.Post(() =>
            {
                try
                {
                    NotificationActions.AddView(button, position);
                }
                catch (Exception ex)
                {
                    Log.Error("LiveDisplay", "Error adding actions: " + ex);
                }
            });
        }

        private void SendInlineResponse_Click(object sender, EventArgs e)
        {
            ImageButton actionButton = sender as ImageButton;
            OpenAction openAction = actionButton.GetTag(DefaultActionIdentificator) as OpenAction;
            openAction.SendInlineResponse(InlineResponse.Text);
            InlineResponse.Text = string.Empty;
            NotificationActions.Visibility = ViewStates.Visible;
            InlineResponseNotificationContainer.Visibility = ViewStates.Invisible;
            // Check if no view has focus:
            View view = NotificationFragment?.Activity?.CurrentFocus;
            if (view != null)
            {
                InputMethodManager imm = (InputMethodManager)NotificationFragment.Activity.GetSystemService(Context.InputMethodService);                
                imm.HideSoftInputFromInputMethod(view.WindowToken, 0);
            }
            SendInlineResponseAvailabityChanged?.Invoke(null, false);
        }

        protected T FindView<T>(int id) where T : View
        {
            return (T)NotificationView.FindViewById(id);
        }
    }
}